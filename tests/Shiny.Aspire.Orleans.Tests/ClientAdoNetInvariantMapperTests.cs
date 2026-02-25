using Shiny.Aspire.Orleans.Client.Internal;
using Shouldly;

namespace Shiny.Aspire.Orleans.Tests;

public class ClientAdoNetInvariantMapperTests
{
    [Theory]
    [InlineData("SqlServerDatabase", "Microsoft.Data.SqlClient")]
    [InlineData("PostgresDatabase", "Npgsql")]
    [InlineData("MySqlDatabase", "MySql.Data.MySqlClient")]
    public void GetInvariant_ReturnsExpectedInvariant(string providerType, string expectedInvariant)
    {
        AdoNetInvariantMapper.GetInvariant(providerType).ShouldBe(expectedInvariant);
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("")]
    [InlineData("sqlserverdatabase")]
    public void GetInvariant_ThrowsForUnsupportedType(string providerType)
    {
        Should.Throw<InvalidOperationException>(() => AdoNetInvariantMapper.GetInvariant(providerType))
            .Message.ShouldContain("Unsupported Orleans ADO.NET provider type");
    }
}
