using Microsoft.EntityFrameworkCore;
using Statusphere.NET;
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

builder.Services.AddHttpClient<StatusphereAuthenticationService>(httpClient =>
{
    httpClient.BaseAddress = new Uri("https://bsky.social");
});

builder.Services.AddHttpClient<DidClient>();

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

app.MapHub<StatusHub>("/hubs/status");

app.Run();
