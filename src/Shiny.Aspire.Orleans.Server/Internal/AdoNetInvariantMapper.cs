namespace Shiny.Aspire.Orleans.Server.Internal;

internal static class AdoNetInvariantMapper
{
    public static string GetInvariant(string providerType) => providerType switch
    {
        "SqlServerDatabase" => "Microsoft.Data.SqlClient",
        "PostgresDatabase" => "Npgsql",
        "MySqlDatabase" => "MySql.Data.MySqlClient",
        _ => throw new InvalidOperationException(
            $"Unsupported Orleans ADO.NET provider type '{providerType}'. " +
            "Supported types are: SqlServerDatabase, PostgresDatabase, MySqlDatabase.")
    };
}
