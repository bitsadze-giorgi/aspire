using Shiny.Aspire.Orleans.Hosting;
using Shiny.Aspire.Orleans.Hosting.Internal;
using Shouldly;

namespace Shiny.Aspire.Orleans.Tests;

public class ScriptLoaderTests
{
    [Theory]
    [InlineData(DatabaseType.SqlServer)]
    [InlineData(DatabaseType.PostgreSQL)]
    [InlineData(DatabaseType.MySql)]
    public void LoadCombinedScript_AllFeatures_ReturnsNonEmptyScript(DatabaseType dbType)
    {
        var script = ScriptLoader.LoadCombinedScript(dbType, OrleansFeature.All);
        script.ShouldNotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(DatabaseType.SqlServer)]
    [InlineData(DatabaseType.PostgreSQL)]
    [InlineData(DatabaseType.MySql)]
    public void LoadCombinedScript_ClusteringOnly_ReturnsScript(DatabaseType dbType)
    {
        var script = ScriptLoader.LoadCombinedScript(dbType, OrleansFeature.Clustering);
        script.ShouldNotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(DatabaseType.SqlServer)]
    [InlineData(DatabaseType.PostgreSQL)]
    [InlineData(DatabaseType.MySql)]
    public void LoadCombinedScript_PersistenceOnly_ReturnsScript(DatabaseType dbType)
    {
        var script = ScriptLoader.LoadCombinedScript(dbType, OrleansFeature.Persistence);
        script.ShouldNotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(DatabaseType.SqlServer)]
    [InlineData(DatabaseType.PostgreSQL)]
    [InlineData(DatabaseType.MySql)]
    public void LoadCombinedScript_RemindersOnly_ReturnsScript(DatabaseType dbType)
    {
        var script = ScriptLoader.LoadCombinedScript(dbType, OrleansFeature.Reminders);
        script.ShouldNotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(DatabaseType.SqlServer)]
    [InlineData(DatabaseType.PostgreSQL)]
    [InlineData(DatabaseType.MySql)]
    public void LoadCombinedScript_AllFeatures_ContainsMoreThanSingleFeature(DatabaseType dbType)
    {
        var allScript = ScriptLoader.LoadCombinedScript(dbType, OrleansFeature.All);
        var clusteringOnly = ScriptLoader.LoadCombinedScript(dbType, OrleansFeature.Clustering);

        allScript.Length.ShouldBeGreaterThan(clusteringOnly.Length);
    }

    [Theory]
    [InlineData(OrleansFeature.Clustering | OrleansFeature.Persistence)]
    [InlineData(OrleansFeature.Clustering | OrleansFeature.Reminders)]
    [InlineData(OrleansFeature.Persistence | OrleansFeature.Reminders)]
    public void LoadCombinedScript_FeatureCombinations_ReturnsScript(OrleansFeature features)
    {
        var script = ScriptLoader.LoadCombinedScript(DatabaseType.PostgreSQL, features);
        script.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void LoadCombinedScript_InvalidDatabaseType_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(
            () => ScriptLoader.LoadCombinedScript((DatabaseType)99, OrleansFeature.All));
    }
}
