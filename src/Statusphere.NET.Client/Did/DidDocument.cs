namespace Statusphere.NET.Client.Did;

public sealed record DidDocument
{
    public required string[] AlsoKnownAs { get; init; }
}