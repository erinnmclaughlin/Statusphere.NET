using System.Text.Json.Serialization;

namespace Statusphere.NET.ATProto;

public abstract record ATRecord(
    [property: JsonPropertyName("$type")] string Type
);
