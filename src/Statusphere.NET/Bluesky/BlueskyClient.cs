using System.Web;

namespace Statusphere.NET.Bluesky;

public class BlueskyActorClient(HttpClient httpClient) : IBlueskyActorClient
{
    /// <summary>
    /// Get detailed profile view of an actor. Does not require auth, but contains relevant metadata with auth.
    /// </summary>
    /// <param name="actor">Handle or DID of account to fetch profile of.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<BlueskyProfile?> GetUserProfileAsync(string actor, CancellationToken cancellationToken)
    {
        const string uri = "xrpc/app.bsky.actor.getProfile";
        return await httpClient.GetFromJsonAsync<BlueskyProfile>(
            $"{uri}?actor={HttpUtility.UrlEncode(actor)}",
            cancellationToken
        );
    }
}