using Shiny.Aspire.Orleans.Hosting;
using Shiny.Aspire.Orleans.Hosting.Internal;
using Shouldly;

namespace Shiny.Aspire.Orleans.Tests;

public class ScriptRunnerTests
{
    [Fact]
    public void SplitBatches_SqlServer_SplitsOnGo()
    {
        var script = "SELECT 1\nGO\nSELECT 2\nGO\nSELECT 3";
        var batches = ScriptRunner.SplitBatches(script, DatabaseType.SqlServer).ToList();

        batches.Count.ShouldBe(3);
        batches[0].Trim().ShouldBe("SELECT 1");
        batches[1].Trim().ShouldBe("SELECT 2");
        batches[2].Trim().ShouldBe("SELECT 3");
    }

    [Fact]
    public void SplitBatches_SqlServer_GoIsCaseInsensitive()
    {
        var script = "SELECT 1\ngo\nSELECT 2\nGo\nSELECT 3";
        var batches = ScriptRunner.SplitBatches(script, DatabaseType.SqlServer).ToList();

        batches.Count.ShouldBe(3);
    }

    [Fact]
    public void SplitBatches_SqlServer_GoWithWhitespace()
    {
        var script = "SELECT 1\n  GO  \nSELECT 2";
        var batches = ScriptRunner.SplitBatches(script, DatabaseType.SqlServer).ToList();

        batches.Count.ShouldBe(2);
        batches[0].Trim().ShouldBe("SELECT 1");
        batches[1].Trim().ShouldBe("SELECT 2");
    }

    [Fact]
    public void SplitBatches_SqlServer_NoGo_ReturnsSingleBatch()
    {
        var script = "SELECT 1; SELECT 2;";
        var batches = ScriptRunner.SplitBatches(script, DatabaseType.SqlServer).ToList();

        batches.Count.ShouldBe(1);
        batches[0].ShouldBe(script);
    }

    [Fact]
    public void SplitBatches_PostgreSQL_ReturnsSingleBatch()
    {
        var script = "SELECT 1;\nSELECT 2;\nSELECT 3;";
        var batches = ScriptRunner.SplitBatches(script, DatabaseType.PostgreSQL).ToList();

        batches.ShouldHaveSingleItem();
        batches[0].ShouldBe(script);
    }

    [Fact]
    public void SplitBatches_MySql_ReturnsSingleBatch()
    {
        var script = "SELECT 1;\nSELECT 2;\nSELECT 3;";
        var batches = ScriptRunner.SplitBatches(script, DatabaseType.MySql).ToList();

        batches.ShouldHaveSingleItem();
        batches[0].ShouldBe(script);
    }

    [Fact]
    public void SplitBatches_SqlServer_GoInMiddleOfLine_DoesNotSplit()
    {
        var script = "SELECT 'GO' FROM Table1";
        var batches = ScriptRunner.SplitBatches(script, DatabaseType.SqlServer).ToList();

        // GO must be on its own line to be a batch separator
        batches.ShouldHaveSingleItem();
    }

    [Fact]
    public void SplitBatches_SqlServer_EmptyScript_ReturnsEmptyBatches()
    {
        var batches = ScriptRunner.SplitBatches("", DatabaseType.SqlServer).ToList();

        batches.ShouldHaveSingleItem();
        batches[0].ShouldBeEmpty();
    }
}
