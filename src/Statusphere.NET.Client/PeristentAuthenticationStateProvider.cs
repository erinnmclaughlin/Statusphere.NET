using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Statusphere.NET.Client;

// This is a client-side AuthenticationStateProvider that determines the user's authentication state by
// looking for data persisted in the page when it was rendered on the server. This authentication state will
// be fixed for the lifetime of the WebAssembly application. So, if the user needs to log in or out, a full
// page reload is required.
//
// This only provides a user info for display purposes. It does not actually include any tokens
// that authenticate to the server when making subsequent requests. That works separately using a
// cookie that will be included on HttpClient requests to the server.
internal class PersistentAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly AuthenticationState _authenticationState;

    public PersistentAuthenticationStateProvider(PersistentComponentState state)
    {
        if (!state.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) || userInfo is null)
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            _authenticationState = new AuthenticationState(anonymousUser);
            return;
        }

        var identity = new ClaimsIdentity(nameof(PersistentAuthenticationStateProvider));
        identity.AddClaims([
            new(ClaimTypes.NameIdentifier, userInfo.Did),
            new(ClaimTypes.Name, userInfo.DisplayName),
            new("handle", userInfo.Handle)
        ]);

        _authenticationState = new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(_authenticationState);
}
