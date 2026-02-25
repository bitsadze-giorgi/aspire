using System.Reflection;

namespace Shiny.Aspire.Orleans.Hosting.Internal;

internal static class ScriptLoader
{
    private static readonly Assembly Assembly = typeof(ScriptLoader).Assembly;

    public static string LoadCombinedScript(DatabaseType dbType, OrleansFeature features)
    {
        var scripts = new List<string>();

        var mainScript = LoadScript(dbType, "Main");
        if (mainScript != null)
            scripts.Add(mainScript);

        if (features.HasFlag(OrleansFeature.Clustering))
        {
            var script = LoadScript(dbType, "Clustering");
            if (script != null)
                scripts.Add(script);
        }

        if (features.HasFlag(OrleansFeature.Persistence))
        {
            var script = LoadScript(dbType, "Persistence");
            if (script != null)
                scripts.Add(script);
        }

        if (features.HasFlag(OrleansFeature.Reminders))
        {
            var script = LoadScript(dbType, "Reminders");
            if (script != null)
                scripts.Add(script);
        }

        return string.Join(Environment.NewLine, scripts);
    }

    private static string? LoadScript(DatabaseType dbType, string scriptName)
    {
        var dbFolder = dbType switch
        {
            DatabaseType.SqlServer => "SqlServer",
            DatabaseType.PostgreSQL => "PostgreSQL",
            DatabaseType.MySql => "MySql",
            _ => throw new ArgumentOutOfRangeException(nameof(dbType))
        };

        var resourceName = $"Shiny.Aspire.Orleans.Hosting.Scripts.{dbFolder}.{scriptName}.sql";
        using var stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            return null;

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
