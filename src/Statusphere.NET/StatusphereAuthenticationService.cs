using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Statusphere.NET.ATProto;
using Statusphere.NET.Bluesky;

namespace Statusphere.NET;

public class StatusphereAuthenticationService(IATProtoClient atProto, IBlueskyActorClient bluesky)
{
    public async Task SignInAsync(HttpContext context, string identifier, string password)
    {
        var session = await atProto.CreateSession(identifier, password);
        var profile = await bluesky.GetUserProfileAsync(session.Did, session.AccessToken);

        var identity = new ClaimsIdentity("Bluesky");
        identity.AddClaims([
            new Claim(ClaimTypes.NameIdentifier, session.Did),
            new Claim(ClaimTypes.Name, profile?.DisplayName ?? session.Handle),
            new Claim("handle", session.Handle),
            new Claim("access_token", session.AccessToken),
            new Claim("refresh_token", session.RefreshToken)
        ]);
        
        if (session.Email is not null)
            identity.AddClaim(new Claim(ClaimTypes.Email, session.Email));
        
        await context.SignInAsync(new ClaimsPrincipal(identity));
    }
}