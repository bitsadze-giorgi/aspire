namespace Shiny.Aspire.Orleans.Client;

public static class OrleansClientExtensions
{
    /// <summary>
    /// Configures the Orleans client with ADO.NET-backed clustering.
    /// Provider configuration is resolved automatically from Aspire-injected configuration
    /// via registered Orleans provider builders (PostgresDatabase, SqlServerDatabase, MySqlDatabase).
    /// </summary>
    public static IClientBuilder UseAdoNetClient(this IClientBuilder clientBuilder) => clientBuilder;
}
