using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Shiny.Aspire.Orleans.Client.Internal;

[assembly: RegisterProvider("PostgresDatabase", "Clustering", "Silo", typeof(DatabaseClusteringClientProviderBuilder))]
[assembly: RegisterProvider("SqlServerDatabase", "Clustering", "Silo", typeof(DatabaseClusteringClientProviderBuilder))]
[assembly: RegisterProvider("MySqlDatabase", "Clustering", "Silo", typeof(DatabaseClusteringClientProviderBuilder))]
[assembly: RegisterProvider("PostgresDatabase", "Clustering", "Client", typeof(DatabaseClusteringClientProviderBuilder))]
[assembly: RegisterProvider("SqlServerDatabase", "Clustering", "Client", typeof(DatabaseClusteringClientProviderBuilder))]
[assembly: RegisterProvider("MySqlDatabase", "Clustering", "Client", typeof(DatabaseClusteringClientProviderBuilder))]

namespace Orleans.Hosting;

internal sealed class DatabaseClusteringClientProviderBuilder : IProviderBuilder<ISiloBuilder>, IProviderBuilder<IClientBuilder>
{
    public void Configure(ISiloBuilder builder, string? name, IConfigurationSection configurationSection)
    {
        builder.UseAdoNetClustering((OptionsBuilder<AdoNetClusteringSiloOptions> optionsBuilder) =>
            optionsBuilder.Configure<IServiceProvider>((options, services) =>
            {
                ConfigureOptions(configurationSection, services, out var invariant, out var connectionString);
                options.Invariant = invariant;
                options.ConnectionString = connectionString;
            }));
    }

    public void Configure(IClientBuilder builder, string? name, IConfigurationSection configurationSection)
    {
        builder.UseAdoNetClustering((OptionsBuilder<AdoNetClusteringClientOptions> optionsBuilder) =>
            optionsBuilder.Configure<IServiceProvider>((options, services) =>
            {
                ConfigureOptions(configurationSection, services, out var invariant, out var connectionString);
                options.Invariant = invariant;
                options.ConnectionString = connectionString;
            }));
    }

    static void ConfigureOptions(IConfigurationSection configurationSection, IServiceProvider services, out string invariant, out string? connectionString)
    {
        var providerType = configurationSection["ProviderType"]!;
        invariant = AdoNetInvariantMapper.GetInvariant(providerType);

        connectionString = null;
        var serviceKey = configurationSection["ServiceKey"];
        if (!string.IsNullOrEmpty(serviceKey))
        {
            connectionString = services.GetRequiredService<IConfiguration>().GetConnectionString(serviceKey);
        }
    }
}
