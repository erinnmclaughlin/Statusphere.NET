using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using Statusphere.NET.Client;
using System.Diagnostics;
using System.Security.Claims;

namespace Statusphere.NET;

// This is a server-side AuthenticationStateProvider that revalidates the security stamp for the connected user
// every 30 minutes an interactive circuit is connected. It also uses PersistentComponentState to flow the
// authentication state to the client which is then fixed for the lifetime of the WebAssembly application.
public class PersistingRevalidatingAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly PersistentComponentState _state;
    private readonly PersistingComponentStateSubscription _subscription;

    private Task<AuthenticationState>? _authenticationStateTask;

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    public PersistingRevalidatingAuthenticationStateProvider(ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory, PersistentComponentState state) : base(loggerFactory)
    {
        _state = state;

        AuthenticationStateChanged += OnAuthenticationStateChanged;
        _subscription = state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
    }

    // todo: actually implement this
    protected override Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    protected override void Dispose(bool disposing)
    {
        _subscription.Dispose();
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
        base.Dispose(disposing);
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        _authenticationStateTask = task;
    }

    private async Task OnPersistingAsync()
    {
        if (_authenticationStateTask is null)
        {
            throw new UnreachableException($"Authentication state not set in {nameof(OnPersistingAsync)}().");
        }

        var authenticationState = await _authenticationStateTask;
        var principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            var did = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var handle = principal.FindFirstValue("handle");
            var displayName = principal.FindFirstValue(ClaimTypes.Name);

            if (did != null && handle != null)
            {
                _state.PersistAsJson(nameof(UserInfo), new UserInfo
                {
                    Did = did,
                    Handle = handle,
                    DisplayName = displayName ?? handle
                });
            }
        }
    }
}
