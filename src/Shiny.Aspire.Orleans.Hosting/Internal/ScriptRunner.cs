using System.Data.Common;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;

namespace Shiny.Aspire.Orleans.Hosting.Internal;

internal static class ScriptRunner
{
    public static async Task RunAsync(
        string connectionString,
        DatabaseType dbType,
        string script,
        CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection(dbType, connectionString);
        await connection.OpenAsync(cancellationToken);

        var batches = SplitBatches(script, dbType);

        foreach (var batch in batches)
        {
            var trimmed = batch.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            await using var command = connection.CreateCommand();
            command.CommandText = trimmed;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static DbConnection CreateConnection(DatabaseType dbType, string connectionString) => dbType switch
    {
        DatabaseType.SqlServer => new SqlConnection(connectionString),
        DatabaseType.PostgreSQL => new NpgsqlConnection(connectionString),
        DatabaseType.MySql => new MySqlConnection(connectionString),
        _ => throw new ArgumentOutOfRangeException(nameof(dbType))
    };

    internal static IEnumerable<string> SplitBatches(string script, DatabaseType dbType)
    {
        if (dbType == DatabaseType.SqlServer)
        {
            // SQL Server uses GO as a batch separator
            return System.Text.RegularExpressions.Regex.Split(
                script,
                @"^\s*GO\s*$",
                System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        // PostgreSQL and MySQL can execute the whole script as one batch
        return [script];
    }
}
