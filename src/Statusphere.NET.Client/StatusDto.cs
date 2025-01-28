namespace Statusphere.NET.Client;

public sealed record StatusDto(string AuthorDid, string? Status, DateTime CreatedAt);
