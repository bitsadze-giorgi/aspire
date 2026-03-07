namespace Shiny.Aspire.Orleans.Server;

public static class OrleansServerExtensions
{
    /// <summary>
    /// Configures the Orleans silo with ADO.NET-backed clustering, grain storage, and reminders.
    /// Provider configuration is resolved automatically from Aspire-injected configuration
    /// via registered Orleans provider builders (PostgresDatabase, SqlServerDatabase, MySqlDatabase).
    /// </summary>
    public static ISiloBuilder UseAdoNet(this ISiloBuilder siloBuilder) => siloBuilder;
}
