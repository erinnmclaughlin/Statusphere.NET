using System.Net.Http.Json;
using System.Web;

namespace Statusphere.NET.Client;

public class DidClient(HttpClient httpClient)
{
    public const string DefaultBaseUri = "https://plc.directory/";

    public async Task<DidDocument> GetDidDocument(string did)
    {
        var document = await httpClient.GetFromJsonAsync<DidDocument>(HttpUtility.UrlEncode(did));
        return document!;
    }
}

public sealed record DidDocument
{
    public required string[] AlsoKnownAs { get; init; }
}