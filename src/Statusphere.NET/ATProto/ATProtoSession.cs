using System.Text.Json.Serialization;

namespace Statusphere.NET.ATProto;

public class ATProtoSession
{
    [JsonPropertyName("accessJwt")]
    public required string AccessToken { get; init; }
    
    [JsonPropertyName("refreshJwt")]
    public required string RefreshToken { get; init; }
    
    [JsonPropertyName("handle")]
    public required string Handle { get; init; }
    
    [JsonPropertyName("did")]
    public required string Did { get; init; }
    
    [JsonPropertyName("email")]
    public string? Email { get; init; }
    
    // other properties exist, but we don't need them
}