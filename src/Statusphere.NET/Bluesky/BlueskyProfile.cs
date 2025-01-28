namespace Statusphere.NET.Bluesky;

public class BlueskyProfile
{
    public required string Did { get; init; }
    public required string Handle { get; init; }
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public string? Avatar { get; init; }
    public string? Banner { get; init; }
    public int FollowersCount { get; init; }
    public int FollowsCount { get; init; }
    public int PostsCount { get; init; }
}