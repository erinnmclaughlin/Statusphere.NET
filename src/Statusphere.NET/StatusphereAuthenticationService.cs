using System.Net.Http.Headers;
using System.Security.Claims;
using System.Web;
using Microsoft.AspNetCore.Authentication;

namespace Statusphere.NET;

public class StatusphereAuthenticationService(HttpClient httpClient)
{
    public async Task SignInAsync(HttpContext context, string identifier, string password)
    {
        const string uri = "xrpc/com.atproto.server.createSession";
        var response = await httpClient.PostAsJsonAsync(uri, new { identifier, password });
        response.EnsureSuccessStatusCode();
            
        var session = await response.Content.ReadFromJsonAsync<CreateSessionResult>();
        
        if (session is null)
            return;

        var profile = await GetUserProfile(session.Did, session.AccessJwt);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.Did),
            new(ClaimTypes.Name, profile?.DisplayName ?? session.Handle),
            new("handle", session.Handle),
            new("access_token", session.AccessJwt),
            new("refresh_token", session.RefreshJwt)
        }; 
        
        if (session.Email is not null)
        {
            claims.Add(new Claim(ClaimTypes.Email, session.Email));
        }

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bluesky"));
        await context.SignInAsync(claimsPrincipal);
        context.User = claimsPrincipal;
    }

    private async Task<UserProfile?> GetUserProfile(string handleOrDid, string authToken)
    {
        const string uri = "xrpc/app.bsky.actor.getProfile";

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{uri}?actor={HttpUtility.UrlEncode(handleOrDid)}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        var responseMessage = await httpClient.SendAsync(requestMessage);
        return await responseMessage.Content.ReadFromJsonAsync<UserProfile>();
    }
    
    private sealed record CreateSessionResult
    {
        public required string AccessJwt { get; init; }
        public required string RefreshJwt { get; init; }
        public required string Handle { get; init; }
        public required string Did { get; init; }
        public string? Email { get; init; }
    }

    private sealed record UserProfile
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
}