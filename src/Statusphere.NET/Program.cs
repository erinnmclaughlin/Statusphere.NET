using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Statusphere.NET;
using Statusphere.NET.ATProto;
using Statusphere.NET.Bluesky;
using Statusphere.NET.Client;
using Statusphere.NET.Components;
using Statusphere.NET.Database;
using Statusphere.NET.Hubs;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Auth stuff:
services.AddAuthentication().AddCookie(); 
services.AddScoped<StatusphereAuthenticationService>();

// Blazor stuff:
services.AddRazorComponents().AddInteractiveServerComponents().AddInteractiveWebAssemblyComponents();
services.AddCascadingAuthenticationState();
services.AddScoped<AuthenticationStateProvider, StatusphereAuthenticationStateProvider>();

// ATProto / Bluesky stuff:
services.AddATClient<IATProtoClient, ATProtoClient>("https://bsky.social");
services.AddATClient<IBlueskyActorClient, BlueskyActorClient>("https://public.api.bsky.app");
services.AddHttpClient<DidClient>(client => client.BaseAddress = new Uri(DidClient.DefaultBaseUri));

// Database stuff:
services.AddDbContextFactory<StatusphereDbContext>(o => o.UseSqlite("Data Source=Statusphere.db"));

// SignalR stuff (so we can update the UI in realtime!):
services.AddSignalR();

// Background job stuff (so we can process events from other users!):
services.AddStatusUpdateListener();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true); 
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Statusphere.NET.Client._Imports).Assembly);

app.MapPost("/logout", async (HttpContext context, [FromForm] string? returnUrl = null) =>
{
    await context.SignOutAsync();
    return TypedResults.LocalRedirect($"~/{returnUrl}");
});

app.MapHub<StatusHub>("/hubs/status");

app.Run();
