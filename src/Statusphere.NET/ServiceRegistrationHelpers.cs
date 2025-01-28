using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Statusphere.NET;

public static class ServiceRegistrationHelpers
{
    public static void AddATClient<TAbstraction, TImplementation>(this IServiceCollection services, string baseUri)
        where TAbstraction : class
        where TImplementation : class, TAbstraction
    {
        services.AddHttpContextAccessor();
        services.TryAddTransient<AuthTokenHandler>(); 
        services
            .AddHttpClient<TAbstraction, TImplementation>(client => client.BaseAddress = new Uri(baseUri))
            .AddHttpMessageHandler<AuthTokenHandler>();
    }

    public static void AddStatusUpdateListener(this IServiceCollection services)
    {
        services.AddHostedService<StatusUpdateSubscription>();
        services.Configure<HostOptions>(x =>
        {
            x.ServicesStartConcurrently = true;
            x.ServicesStopConcurrently = true;
        });
    }
}