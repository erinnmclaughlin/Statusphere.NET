namespace Statusphere.NET.Bluesky;

public interface IBlueskyActorClient
{
    Task<BlueskyProfile?> GetUserProfileAsync(string handleOrDid, string authToken, CancellationToken cancellationToken = default);
}