namespace Statusphere.NET.ATProto;

public sealed class ATProtoClient(HttpClient httpClient) : IATProtoClient
{
    /// <inheritdoc />
    public async Task<ATProtoSession> CreateSession(string identifier, string password, CancellationToken cancellationToken)
    {
        const string uri = "xrpc/com.atproto.server.createSession";
        
        var response = await httpClient.PostAsJsonAsync(uri, new { identifier, password }, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var session = await response.Content.ReadFromJsonAsync<ATProtoSession>(cancellationToken);
        return session!;
    }

    /// <inheritdoc />
    public async Task PutRecord<T>(string repo, string collection, string rkey, T record, CancellationToken cancellationToken) where T : ATRecord
    {
        const string uri = "xrpc/com.atproto.repo.putRecord";
        
        var response = await httpClient.PostAsJsonAsync(uri, new { repo, collection, rkey, record }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}