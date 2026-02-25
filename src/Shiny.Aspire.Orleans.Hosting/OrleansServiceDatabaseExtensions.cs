using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Orleans;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shiny.Aspire.Orleans.Hosting.Internal;

namespace Shiny.Aspire.Orleans.Hosting;

public static class OrleansServiceDatabaseExtensions
{
    public static OrleansService WithDatabaseSetup(
        this OrleansService orleans,
        IResourceBuilder<IResourceWithConnectionString> database,
        OrleansFeature features = OrleansFeature.All)
    {
        var dbType = DatabaseTypeDetector.Detect(database);
        var dbResourceName = database.Resource.Name;

        orleans.Builder.Eventing.Subscribe<ResourceReadyEvent>(
            database.Resource,
            async (@event, cancellationToken) =>
            {
                var logger = @event.Services.GetRequiredService<ILogger<OrleansService>>();
                logger.LogInformation(
                    "Running Orleans database setup scripts for {DatabaseType} on resource '{ResourceName}' (features: {Features})",
                    dbType,
                    dbResourceName,
                    features);

                var connectionString = await database.Resource.GetConnectionStringAsync(cancellationToken);
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        $"Connection string for '{dbResourceName}' was unexpectedly null or empty.");
                }

                var script = ScriptLoader.LoadCombinedScript(dbType, features);
                if (string.IsNullOrWhiteSpace(script))
                {
                    logger.LogWarning("No Orleans SQL scripts found for {DatabaseType}", dbType);
                    return;
                }

                try
                {
                    await ScriptRunner.RunAsync(connectionString, dbType, script, cancellationToken);
                    logger.LogInformation("Orleans database setup completed successfully for '{ResourceName}'", dbResourceName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to run Orleans database setup scripts for '{ResourceName}'", dbResourceName);
                    throw;
                }
            });

        return orleans;
    }
}
