namespace Statusphere.NET.Client;

public sealed record UserInfo
{
    public required string Did { get; init; }
    public required string DisplayName { get; init; }
    public required string Handle { get; init; }
}
