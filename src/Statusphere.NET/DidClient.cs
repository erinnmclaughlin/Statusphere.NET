using System.Web;

namespace Statusphere.NET;

public class DidClient
{
    private readonly HttpClient _httpClient;
    
    public const string DefaultBaseUri = "https://plc.directory";

    public DidClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress ??= new Uri(DefaultBaseUri);
    }
    
    public async Task<DidDocument> GetDidDocument(string did)
    {
        var encoded = HttpUtility.UrlEncode(did);
        var document = await _httpClient.GetFromJsonAsync<DidDocument>(encoded);
        return document!;
    }
}

public sealed record DidDocument
{
    public required string[] AlsoKnownAs { get; init; }
}