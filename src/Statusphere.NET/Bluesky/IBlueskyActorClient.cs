namespace Statusphere.NET.Bluesky;

public interface IBlueskyActorClient
{
    Task<BlueskyProfile?> GetUserProfileAsync(string handleOrDid, CancellationToken cancellationToken = default);
}