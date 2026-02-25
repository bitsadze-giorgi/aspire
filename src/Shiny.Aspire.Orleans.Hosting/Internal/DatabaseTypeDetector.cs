using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Shiny.Aspire.Orleans.Hosting.Internal;

internal static class DatabaseTypeDetector
{
    public static DatabaseType Detect(IResourceBuilder<IResourceWithConnectionString> database)
    {
        var resourceType = database.Resource.GetType().Name;

        return resourceType switch
        {
            _ when resourceType.StartsWith("Postgres", StringComparison.OrdinalIgnoreCase) => DatabaseType.PostgreSQL,
            _ when resourceType.StartsWith("SqlServer", StringComparison.OrdinalIgnoreCase) => DatabaseType.SqlServer,
            _ when resourceType.StartsWith("MySql", StringComparison.OrdinalIgnoreCase) => DatabaseType.MySql,
            _ => throw new InvalidOperationException(
                $"Unsupported database resource type '{resourceType}'. " +
                "Supported types are PostgreSQL, SQL Server, and MySQL.")
        };
    }
}
