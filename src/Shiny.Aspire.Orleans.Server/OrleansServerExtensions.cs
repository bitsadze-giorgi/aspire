using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Shiny.Aspire.Orleans.Server.Internal;

namespace Shiny.Aspire.Orleans.Server;

public static class OrleansServerExtensions
{
    private static readonly HashSet<string> AdoNetProviderTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "SqlServerDatabase",
        "PostgresDatabase",
        "MySqlDatabase"
    };

    public static IHostApplicationBuilder UseOrleansWithAdoNet(this IHostApplicationBuilder builder)
    {
        var config = builder.Configuration;

        builder.UseOrleans(siloBuilder =>
        {
            ConfigureClustering(siloBuilder, config);
            ConfigureGrainStorage(siloBuilder, config);
            ConfigureReminders(siloBuilder, config);
        });

        return builder;
    }

    private static void ConfigureClustering(global::Orleans.Hosting.ISiloBuilder siloBuilder, IConfiguration config)
    {
        var providerType = config["Orleans:Clustering:ProviderType"];
        if (string.IsNullOrEmpty(providerType) || !AdoNetProviderTypes.Contains(providerType))
            return;

        var serviceKey = config["Orleans:Clustering:ServiceKey"];
        var connectionString = config.GetConnectionString(serviceKey!);
        var invariant = AdoNetInvariantMapper.GetInvariant(providerType);

        siloBuilder.UseAdoNetClustering(options =>
        {
            options.ConnectionString = connectionString;
            options.Invariant = invariant;
        });
    }

    private static void ConfigureGrainStorage(global::Orleans.Hosting.ISiloBuilder siloBuilder, IConfiguration config)
    {
        var storageSection = config.GetSection("Orleans:GrainStorage");
        foreach (var child in storageSection.GetChildren())
        {
            var providerType = child["ProviderType"];
            if (string.IsNullOrEmpty(providerType) || !AdoNetProviderTypes.Contains(providerType))
                continue;

            var serviceKey = child["ServiceKey"];
            var connectionString = config.GetConnectionString(serviceKey!);
            var invariant = AdoNetInvariantMapper.GetInvariant(providerType);

            siloBuilder.AddAdoNetGrainStorage(child.Key, options =>
            {
                options.ConnectionString = connectionString;
                options.Invariant = invariant;
            });
        }
    }

    private static void ConfigureReminders(global::Orleans.Hosting.ISiloBuilder siloBuilder, IConfiguration config)
    {
        var providerType = config["Orleans:Reminders:ProviderType"];
        if (string.IsNullOrEmpty(providerType) || !AdoNetProviderTypes.Contains(providerType))
            return;

        var serviceKey = config["Orleans:Reminders:ServiceKey"];
        var connectionString = config.GetConnectionString(serviceKey!);
        var invariant = AdoNetInvariantMapper.GetInvariant(providerType);

        siloBuilder.UseAdoNetReminderService(options =>
        {
            options.ConnectionString = connectionString;
            options.Invariant = invariant;
        });
    }
}
