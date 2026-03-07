using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Shiny.Aspire.Orleans.Server.Internal;

[assembly: RegisterProvider("PostgresDatabase", "Clustering", "Silo", typeof(DatabaseClusteringProviderBuilder))]
[assembly: RegisterProvider("SqlServerDatabase", "Clustering", "Silo", typeof(DatabaseClusteringProviderBuilder))]
[assembly: RegisterProvider("MySqlDatabase", "Clustering", "Silo", typeof(DatabaseClusteringProviderBuilder))]

namespace Orleans.Hosting;

internal sealed class DatabaseClusteringProviderBuilder : IProviderBuilder<ISiloBuilder>
{
    public void Configure(ISiloBuilder builder, string? name, IConfigurationSection configurationSection)
    {
        builder.UseAdoNetClustering((OptionsBuilder<AdoNetClusteringSiloOptions> optionsBuilder) =>
            optionsBuilder.Configure<IServiceProvider>((options, services) =>
            {
                var providerType = configurationSection["ProviderType"]!;
                options.Invariant = AdoNetInvariantMapper.GetInvariant(providerType);

                var serviceKey = configurationSection["ServiceKey"];
                if (!string.IsNullOrEmpty(serviceKey))
                {
                    options.ConnectionString = services.GetRequiredService<IConfiguration>().GetConnectionString(serviceKey);
                }
            }));
    }
}
