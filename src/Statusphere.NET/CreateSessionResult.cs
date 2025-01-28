namespace Statusphere.NET;

public sealed record CreateSessionResult
{
    public required string AccessJwt { get; init; }
    public required string RefreshJwt { get; init; }
    public required string Handle { get; init; }
    public required string Did { get; init; }
    public string? Email { get; init; }
}