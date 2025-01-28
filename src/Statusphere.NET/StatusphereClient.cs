using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Web;
using Statusphere.NET.Database;

namespace Statusphere.NET;

public class StatusphereClient(HttpClient httpClient)
{
    public async Task<CreateSessionResult?> CreateSession(string identifier, string password)
    {
        const string uri = "xrpc/com.atproto.server.createSession";
        
        var response = await httpClient.PostAsJsonAsync(uri, new { identifier, password });
        response.EnsureSuccessStatusCode();
            
        return await response.Content.ReadFromJsonAsync<CreateSessionResult>();
    }
    
    public async Task<UserProfile?> GetUserProfile(string handleOrDid, string authToken)
    {
        const string uri = "xrpc/app.bsky.actor.getProfile";

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{uri}?actor={HttpUtility.UrlEncode(handleOrDid)}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        var responseMessage = await httpClient.SendAsync(requestMessage);
        return await responseMessage.Content.ReadFromJsonAsync<UserProfile>();
    }

    public async Task UpdateStatus(Status status)
    {
        const string uri = "xrpc/com.atproto.repo.putRecord";

        var response = await httpClient.PostAsJsonAsync(uri, new
        {
            repo = status.AuthorDid,
            collection = "xyz.statusphere.status",
            rkey = status.Uri,
            record = InternalStatusDto.Create(status)
        });
        
        response.EnsureSuccessStatusCode();
    }

    private record InternalStatusDto
    {
        [JsonPropertyName("$type")]
        public string Type { get; private init; } = "xyz.statusphere.status";
        public required string? Status { get; set; }
        public required string CreatedAt { get; set; }

        public static InternalStatusDto Create(Status status)
        {
            return new InternalStatusDto
            {
                Status = status.Value,
                CreatedAt = status.CreatedAt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture)
            };
        }
    }
}

public class AuthTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (httpContextAccessor.HttpContext?.User is { Identity.IsAuthenticated: true } authenticatedUser)
        {
            var authToken = authenticatedUser.FindFirstValue("access_token");

            if (authToken is not null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }
        
        return base.SendAsync(request, cancellationToken);
    }
}