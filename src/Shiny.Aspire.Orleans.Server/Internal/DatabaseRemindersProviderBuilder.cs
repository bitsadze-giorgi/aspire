using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Shiny.Aspire.Orleans.Server.Internal;

[assembly: RegisterProvider("PostgresDatabase", "Reminders", "Silo", typeof(DatabaseRemindersProviderBuilder))]
[assembly: RegisterProvider("SqlServerDatabase", "Reminders", "Silo", typeof(DatabaseRemindersProviderBuilder))]
[assembly: RegisterProvider("MySqlDatabase", "Reminders", "Silo", typeof(DatabaseRemindersProviderBuilder))]

namespace Orleans.Hosting;

internal sealed class DatabaseRemindersProviderBuilder : IProviderBuilder<ISiloBuilder>
{
    public void Configure(ISiloBuilder builder, string? name, IConfigurationSection configurationSection)
    {
        builder.UseAdoNetReminderService((OptionsBuilder<AdoNetReminderTableOptions> optionsBuilder) =>
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
