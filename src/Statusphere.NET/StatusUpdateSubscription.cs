using FishyFlip;
using FishyFlip.Events;

namespace Statusphere.NET;

public sealed class StatusUpdateSubscription(ILogger<StatusUpdateSubscription> logger) : BackgroundService
{
    private readonly ATWebSocketProtocol _atp = new ATWebSocketProtocolBuilder().WithLogger(logger).Build();
    private readonly ILogger<StatusUpdateSubscription> _logger = logger;

    public override void Dispose()
    {
        _atp.OnSubscribedRepoMessage -= HandleMessage;
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _atp.OnSubscribedRepoMessage += HandleMessage;
        
        await _atp.StartSubscribeReposAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, CancellationToken.None);
        }
        
        await _atp.StopSubscriptionAsync();
    }

    private void HandleMessage(object? _, SubscribedRepoEventArgs args)
    {
        var message = args.Message;

        if (message.Commit?.Repo is null)
        {
            return;
        }

        if (message.Record is { Type: "xyz.statusphere.status" or "xyz.statusphere.status#main" })
        {
            _logger.LogInformation("Json: {EventJson}", message.Record.ToJson());
        }
    }
}
