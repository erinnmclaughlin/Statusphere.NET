using System.Net.Http.Headers;
using System.Web;

namespace Statusphere.NET.Bluesky;

public class BlueskyActorClient(HttpClient httpClient) : IBlueskyActorClient
{
    public async Task<BlueskyProfile?> GetUserProfileAsync(string handleOrDid, string authToken, CancellationToken cancellationToken)
    {
        const string uri = "xrpc/app.bsky.actor.getProfile";
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{uri}?actor={HttpUtility.UrlEncode(handleOrDid)}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        var responseMessage = await httpClient.SendAsync(requestMessage, cancellationToken);
        return await responseMessage.Content.ReadFromJsonAsync<BlueskyProfile>(cancellationToken);
    }
}