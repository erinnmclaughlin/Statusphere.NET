using System.Globalization;
using System.Text.Json.Serialization;
using Statusphere.NET.ATProto;
using Statusphere.NET.Database;

namespace Statusphere.NET;

public record StatusRecord() : ATRecord("xyz.statusphere.status")
{
    [JsonPropertyName("status")]
    public required string? Status { get; init; }
    
    [JsonPropertyName("createdAt")]
    public required string CreatedAt { get; init; }

    public static StatusRecord FromDatabaseEntity(Status status) => new()
    {
        Status = status.Value,
        CreatedAt = status.CreatedAt.ToString("o", CultureInfo.InvariantCulture)
    };
}
