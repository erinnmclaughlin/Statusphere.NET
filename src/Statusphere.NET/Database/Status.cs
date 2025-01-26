using System.ComponentModel.DataAnnotations;

namespace Statusphere.NET.Database;

public class Status
{
    [Key]
    public required string Uri { get; init; }
    public required string AuthorDid { get; init; }
    public string? Value { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}