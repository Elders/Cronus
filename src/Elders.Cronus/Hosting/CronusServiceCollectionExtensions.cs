using System;
using System.Diagnostics;
using System.Linq;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.Cluster.Job;
using Elders.Cronus.Discoveries;
using Elders.Cronus.Hosting.Heartbeat;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Elders.Cronus;

public static class CronusServiceCollectionExtensions
{
    /// <summary>
    /// // Adds Cronus core services
    /// </summary>
    public static IServiceCollection AddCronus(this IServiceCollection services, IConfiguration configuration)
    {
        return AddCronus(services, new CronusServicesProvider(services, configuration));
    }

    /// <summary>
    /// // Adds Cronus core services
    /// </summary>
    public static IServiceCollection AddCronus(this IServiceCollection services, CronusServicesProvider cronusServicesProvider)
    {
        services.AddBooter();
        services.AddOpenTelemetry();
        services.AddTenantSupport();
        services.AddCronusHostOptions();
        services.AddDefaultSubscribers(cronusServicesProvider);
        services.AddInMemoryLock();
        services.AddJobManager();

        var discoveryFinder = new DiscoveryScanner();
        var discoveryContext = new DiscoveryContext(AssemblyLoader.Assemblies.Values, cronusServicesProvider.Configuration);
        var discoveryResults = discoveryFinder.Scan(discoveryContext);

        foreach (var result in discoveryResults)
            cronusServicesProvider.HandleDiscoveredModel(result);

        services.AddCronusHeartbeat();

        return services;
    }

    internal static IServiceCollection AddJobManager(this IServiceCollection services)
    {
        services.AddSingleton<JobManager>();

        return services;
    }

    internal static IServiceCollection AddCronusHeartbeat(this IServiceCollection services)
    {
        services.AddOptions<HeartbeatOptions, HeartbeaOptionsProvider>();
        services.AddSingleton<IHeartbeat, CronusHeartbeat>();
        services.AddHostedService<CronusHeartbeatService>();

        return services;
    }

    public static IServiceCollection AddInMemoryLock(this IServiceCollection services)
    {
        services.AddSingleton<ILock, InMemoryLockWithTTL>();

        return services;
    }

    internal static IServiceCollection AddBooter(this IServiceCollection services)
    {
        return services.AddSingleton<CronusBooter>();
    }

    internal static IServiceCollection AddOpenTelemetry(this IServiceCollection services)
    {
        // https://github.com/dotnet/aspnetcore/blob/f3f9a1cdbcd06b298035b523732b9f45b1408461/src/Hosting/Hosting/src/WebHostBuilder.cs#L334
        // By default aspnet core registers a DiagnosticListener and if we add our own you will loose the http insights
        // However, for worker services we need to register our own Listener.
        // I am sure there is a better way for doing the setup so please if you end up here and you have problems please fix it.
        if (services.Any(x => x.ServiceType == typeof(DiagnosticListener)) == false)
        {
            services.AddSingleton<DiagnosticListener>(new DiagnosticListener("cronus"));

            services.AddSingleton<ActivitySource>(new ActivitySource("Elders.Cronus", "10.0.0"));
        }

        return services;
    }

    internal static IServiceCollection AddTenantSupport(this IServiceCollection services)
    {
        services.AddOptions<TenantsOptions, TenantsOptionsProvider>();
        services.AddTransient(typeof(SingletonPerTenant<>));
        services.AddSingleton(typeof(SingletonPerTenantContainer<>));

        services.AddSingleton<ICronusContextAccessor, CronusContextAccessor>();
        services.AddSingleton<DefaultCronusContextFactory>();

        return services;
    }

    internal static IServiceCollection AddCronusHostOptions(this IServiceCollection services)
    {
        services.AddOptions();
        services.AddOptions<CronusHostOptions, CronusHostOptionsProvider>();
        services.AddOptions<BoundedContext, BoundedContextProvider>();

        return services;
    }

    /// <summary>
    /// Adds a service which lifestyle is singleton per tenant
    /// </summary>
    public static IServiceCollection AddTenantSingleton<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddTransient<TImplementation>();
        services.AddTransient<TService>(provider => provider.GetRequiredService<SingletonPerTenant<TImplementation>>().Get());

        return services;
    }

    /// <summary>
    /// Replaces a service with a new one
    /// </summary>
    public static IServiceCollection Replace<TService, TImplementation>(this IServiceCollection services)
        where TImplementation : class, TService
    {
        return Replace(services, typeof(TService), typeof(TImplementation));
    }

    public static IServiceCollection Replace(this IServiceCollection services, Type serviceType, Type implementationType)
    {
        var descriptorToRemove = services.FirstOrDefault(d => d.ServiceType == serviceType);
        if (descriptorToRemove is null)
        {
            throw new ArgumentException($"Service of type {serviceType.Name} is not registered and cannot be replaced.");
        }
        services.Remove(descriptorToRemove);
        var descriptorToAdd = new ServiceDescriptor(serviceType, implementationType, descriptorToRemove.Lifetime);
        services.Add(descriptorToAdd);

        return services;
    }

    public static IServiceCollection AddOptions<TOptions, TOptionsProvider>(this IServiceCollection services)
        where TOptions : class, new()
        where TOptionsProvider : CronusOptionsProviderBase<TOptions>
    {
        services.AddSingleton<IConfigureOptions<TOptions>, TOptionsProvider>();
        services.AddSingleton<IOptionsChangeTokenSource<TOptions>, TOptionsProvider>();
        services.AddSingleton<IOptionsFactory<TOptions>, TOptionsProvider>();

        return services;
    }
}
