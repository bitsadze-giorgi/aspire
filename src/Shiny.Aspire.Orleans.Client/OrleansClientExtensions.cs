using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Shiny.Aspire.Orleans.Client.Internal;

namespace Shiny.Aspire.Orleans.Client;

public static class OrleansClientExtensions
{
    private static readonly HashSet<string> AdoNetProviderTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "SqlServerDatabase",
        "PostgresDatabase",
        "MySqlDatabase"
    };

    public static IHostApplicationBuilder UseOrleansClientWithAdoNet(this IHostApplicationBuilder builder)
    {
        var config = builder.Configuration;

        builder.UseOrleans(siloBuilder =>
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
        });

        return builder;
    }
}
