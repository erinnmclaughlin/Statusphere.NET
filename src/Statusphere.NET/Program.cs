using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Statusphere.NET;
using Statusphere.NET.Client;
using Statusphere.NET.Components;
using Statusphere.NET.Database;
using Statusphere.NET.Hubs;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthentication().AddCookie(); 
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddHttpClient<StatusphereClient>(httpClient =>
{
    httpClient.BaseAddress = new Uri("https://bsky.social");
}).AddHttpMessageHandler<AuthTokenHandler>();


builder.Services.AddScoped<StatusphereAuthenticationService>();
builder.Services.AddScoped<DidClient>();

builder.Services.AddSignalR();

services.AddDbContextFactory<StatusphereDbContext>(o => o.UseSqlite("Data Source=Statusphere.db"));

builder.Services.Configure<HostOptions>(x =>
{
    x.ServicesStartConcurrently = true;
    x.ServicesStopConcurrently = true;
});
services.AddHostedService<StatusUpdateSubscription>();

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

app.MapGet("/api/statuses", async (StatusphereDbContext dbContext, [FromQuery] int limit = 10) =>
{
    return await dbContext.Statuses
        .OrderByDescending(x => x.CreatedAt)
        .Select(x => new StatusDto(x.AuthorDid, x.Value, x.CreatedAt))
        .Take(limit)
        .ToListAsync();
});

app.MapPost("/logout", async (HttpContext context, [FromForm] string? returnUrl = null) =>
{
    await context.SignOutAsync();
    return TypedResults.LocalRedirect($"~/{returnUrl}");
});

app.MapHub<StatusHub>("/hubs/status");

app.Run();
