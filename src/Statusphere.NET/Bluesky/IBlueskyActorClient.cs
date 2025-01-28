namespace Statusphere.NET.Bluesky;

public interface IBlueskyActorClient
{
    /// <summary>
    /// Get detailed profile view of an actor. Does not require auth, but contains relevant metadata with auth.
    /// </summary>
    /// <param name="actor">Handle or DID of account to fetch profile of.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<BlueskyProfile?> GetUserProfileAsync(string actor, CancellationToken cancellationToken = default);
}