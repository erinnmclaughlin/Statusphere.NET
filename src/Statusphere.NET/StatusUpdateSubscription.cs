using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PeterO.Cbor;
using Statusphere.NET.Client;
using Statusphere.NET.Database;
using Statusphere.NET.Hubs;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Statusphere.NET;

public sealed class StatusUpdateSubscription(IDbContextFactory<StatusphereDbContext> dbContextFactory, ILogger<StatusUpdateSubscription> logger, IHubContext<StatusHub> statusHubContext) : BackgroundService
{
    private readonly IDbContextFactory<StatusphereDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<StatusUpdateSubscription> _logger = logger;
    private readonly IHubContext<StatusHub> _statusHubContext = statusHubContext;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var ws = new ClientWebSocket();

        _logger.LogInformation("Connecting to WebSocket...");
        await ws.ConnectAsync(new Uri("wss://bsky.network/xrpc/com.atproto.sync.subscribeRepos"), stoppingToken);

        while (!stoppingToken.IsCancellationRequested && ws.State is WebSocketState.Open)
        {
            var pipe = new Pipe();
            ValueWebSocketReceiveResult receiveResult;

            do
            {
                var memory = pipe.Writer.GetMemory(8192);
                receiveResult = await ws.ReceiveAsync(memory, stoppingToken);

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogWarning("Server closed the connection.");
                    return;
                }

                if (receiveResult.MessageType == WebSocketMessageType.Binary)
                {
                    pipe.Writer.Advance(receiveResult.Count);
                }
            }
            while (!receiveResult.EndOfMessage);

            await pipe.Writer.FlushAsync(stoppingToken);
            await ProcessMessageAsync(pipe, stoppingToken);
        }

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Shutting down", stoppingToken);
    }

    private async Task ProcessMessageAsync(Pipe pipe, CancellationToken cancellationToken)
    {
        var result = await pipe.Reader.ReadAsync(cancellationToken);

        var byteArray = result.Buffer.ToArray();

        if (byteArray.Length < 2)
        {
            _logger.LogDebug("Skipping short frame of length={MessageLength}", byteArray.Length);
            return;
        }

        CBORObject[] objects;

        try
        {
            objects = CBORObject.DecodeSequenceFromBytes(byteArray, new CBOREncodeOptions("useIndefLengthStrings=true;float64=true;allowduplicatekeys=true;allowEmpty=true"));
        }
        catch (CBORException)
        {
            //_logger.LogWarning("Invalid CBOR in message. Skipping frame...");
            return;
        }

        if (objects.Length != 2)
            return;

        var blocks = objects[1].GetOrDefault("blocks", null);

        if (blocks is null) return;

        byte[] frameBytes;

        try
        {
            frameBytes = blocks.GetByteString();
        }
        catch
        {
            return;
        }

        var dictionary = new Dictionary<string, CBORObject>();
        var isStatusType = false;
        foreach (var (cid, data) in CarParser.ReadCarBlocks(frameBytes))
        {
            try
            {
                using var blockStream = new MemoryStream(data);
                var blockObj = CBORObject.Read(blockStream);

                dictionary.Add(cid, blockObj);

                var type = blockObj["$type"]?.AsString();

                if (type == "xyz.statusphere.status")
                {
                    isStatusType = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse DAG-CBOR string.");
            }
        }

        if (isStatusType)
        {
            StatusMessage? statusMessage = null;
            EventMetaData? metaData = null;

            foreach (var (_, value) in dictionary)
            {
                if (value["$type"]?.AsString() == "xyz.statusphere.status")
                {
                    statusMessage = JsonSerializer.Deserialize<StatusMessage>(value.ToJSONString());
                }

                if (value["did"]?.AsString() is not null)
                {
                    metaData = JsonSerializer.Deserialize<EventMetaData>(value.ToJSONString());
                }
            }

            if (statusMessage is not null && metaData is not null)
            {
                var status = await PersistMessage(statusMessage, metaData);

                var dto = new StatusDto(status.AuthorDid, status.Value, status.CreatedAt);
                await _statusHubContext.Clients.All.SendAsync("StatusCreated", dto, CancellationToken.None);
            }
        }
    }

    private async Task<Status> PersistMessage(StatusMessage message, EventMetaData eventData)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        var status = new Status
        {
            Uri = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.Parse(message.CreatedAt),
            Value = message.Status,
            AuthorDid = eventData.Did
        };
        await dbContext.Statuses.AddAsync(status);
        await dbContext.SaveChangesAsync();
        return status;
    }
}

public class StatusMessage
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = "";
}

public class EventMetaData
{
    [JsonPropertyName("did")]
    public string Did { get; set; } = "";

    [JsonPropertyName("rev")]
    public string? Rev { get; set; }

    [JsonPropertyName("sig")]
    public string? Sig { get; set; }

    [JsonPropertyName("data")]
    public string? Data { get; set; }

    [JsonPropertyName("version")]
    public int? Version { get; set; }
}