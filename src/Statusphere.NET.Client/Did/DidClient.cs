using System.Net.Http.Json;
using System.Web;

namespace Statusphere.NET.Client.Did;

public class DidClient(HttpClient httpClient) : IDidClient
{
    public const string DefaultBaseUri = "https://plc.directory/";

    public async Task<DidDocument> GetDidDocument(string did)
    {
        var document = await httpClient.GetFromJsonAsync<DidDocument>(HttpUtility.UrlEncode(did));
        return document!;
    }
}