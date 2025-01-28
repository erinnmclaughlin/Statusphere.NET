using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Statusphere.NET;

public class StatusphereAuthenticationService(StatusphereClient statusphereClient)
{
    public async Task SignInAsync(HttpContext context, string identifier, string password)
    {
        var session = await statusphereClient.CreateSession(identifier, password);
        
        if (session is null)
            return;

        var profile = await statusphereClient.GetUserProfile(session.Did, session.AccessJwt);
        
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
}