using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Storage;
using Shiny.Aspire.Orleans.Server.Internal;

[assembly: RegisterProvider("PostgresDatabase", "GrainStorage", "Silo", typeof(DatabaseGrainStorageProviderBuilder))]
[assembly: RegisterProvider("SqlServerDatabase", "GrainStorage", "Silo", typeof(DatabaseGrainStorageProviderBuilder))]
[assembly: RegisterProvider("MySqlDatabase", "GrainStorage", "Silo", typeof(DatabaseGrainStorageProviderBuilder))]

namespace Orleans.Hosting;

internal sealed class DatabaseGrainStorageProviderBuilder : IProviderBuilder<ISiloBuilder>
{
    public void Configure(ISiloBuilder builder, string? name, IConfigurationSection configurationSection)
    {
        builder.AddAdoNetGrainStorage(name!, (OptionsBuilder<AdoNetGrainStorageOptions> optionsBuilder) =>
            optionsBuilder.Configure<IServiceProvider>((options, services) =>
            {
                var providerType = configurationSection["ProviderType"]!;
                options.Invariant = AdoNetInvariantMapper.GetInvariant(providerType);

                var serviceKey = configurationSection["ServiceKey"];
                if (!string.IsNullOrEmpty(serviceKey))
                {
                    options.ConnectionString = services.GetRequiredService<IConfiguration>().GetConnectionString(serviceKey);
                }

                var serializerKey = configurationSection["SerializerKey"];
                if (!string.IsNullOrEmpty(serializerKey))
                {
                    options.GrainStorageSerializer = services.GetRequiredKeyedService<IGrainStorageSerializer>(serializerKey);
                }
            }));
    }
}
