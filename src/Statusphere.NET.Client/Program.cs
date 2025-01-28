using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Statusphere.NET.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(_ => new DidClient(new HttpClient { BaseAddress = new Uri(DidClient.DefaultBaseUri) }));
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

await builder.Build().RunAsync();
