using System.Net.Http.Json;
using System.Web;

namespace Statusphere.NET.Client;

public class DidClient
{
    private readonly HttpClient _httpClient = new();

    public async Task<DidDocument> GetDidDocument(string did)
    {
        var uri = $"https://plc.directory/{HttpUtility.UrlEncode(did)}";
        var document = await _httpClient.GetFromJsonAsync<DidDocument>(uri);
        return document!;
    }
}

public sealed record DidDocument
{
    public required string[] AlsoKnownAs { get; init; }
}