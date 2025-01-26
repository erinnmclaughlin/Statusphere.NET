using System.Web;

namespace Statusphere.NET;

public class DidClient(HttpClient httpClient)
{
    public async Task<DidDocument> GetDidDoc(string did)
    {
        var encoded = HttpUtility.UrlEncode(did);
        var document = await httpClient.GetFromJsonAsync<DidDocument>(encoded);
        return document!;
    }
}

public sealed record DidDocument
{
    public required string Id { get; init; }
    public required string[] AlsoKnownAs { get; init; }
}