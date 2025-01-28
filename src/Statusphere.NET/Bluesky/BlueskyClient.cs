using System.Web;

namespace Statusphere.NET.Bluesky;

public class BlueskyActorClient(HttpClient httpClient) : IBlueskyActorClient
{
    /// <inheritdoc />
    public async Task<BlueskyProfile?> GetUserProfileAsync(string actor, CancellationToken cancellationToken)
    {
        const string uri = "xrpc/app.bsky.actor.getProfile";
        return await httpClient.GetFromJsonAsync<BlueskyProfile>(
            $"{uri}?actor={HttpUtility.UrlEncode(actor)}",
            cancellationToken
        );
    }
}