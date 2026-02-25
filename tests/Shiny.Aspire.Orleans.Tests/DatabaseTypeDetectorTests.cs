using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using NSubstitute;
using Shiny.Aspire.Orleans.Hosting;
using Shiny.Aspire.Orleans.Hosting.Internal;
using Shouldly;

namespace Shiny.Aspire.Orleans.Tests;

public class DatabaseTypeDetectorTests
{
    [Fact]
    public void Detect_PostgresResource_ReturnsPostgreSQL()
    {
        var builder = CreateResourceBuilder<PostgresTestResource>();
        DatabaseTypeDetector.Detect(builder).ShouldBe(DatabaseType.PostgreSQL);
    }

    [Fact]
    public void Detect_SqlServerResource_ReturnsSqlServer()
    {
        var builder = CreateResourceBuilder<SqlServerTestResource>();
        DatabaseTypeDetector.Detect(builder).ShouldBe(DatabaseType.SqlServer);
    }

    [Fact]
    public void Detect_MySqlResource_ReturnsMySql()
    {
        var builder = CreateResourceBuilder<MySqlTestResource>();
        DatabaseTypeDetector.Detect(builder).ShouldBe(DatabaseType.MySql);
    }

    [Fact]
    public void Detect_UnsupportedResource_Throws()
    {
        var builder = CreateResourceBuilder<UnsupportedTestResource>();
        Should.Throw<InvalidOperationException>(() => DatabaseTypeDetector.Detect(builder))
            .Message.ShouldContain("Unsupported database resource type");
    }

    private static IResourceBuilder<IResourceWithConnectionString> CreateResourceBuilder<TResource>()
        where TResource : IResourceWithConnectionString, new()
    {
        var builder = Substitute.For<IResourceBuilder<IResourceWithConnectionString>>();
        builder.Resource.Returns(new TResource());
        return builder;
    }

    private class PostgresTestResource : IResourceWithConnectionString
    {
        public string Name => "postgres-test";
        public ResourceAnnotationCollection Annotations { get; } = new();
        public ReferenceExpression ConnectionStringExpression =>
            ReferenceExpression.Create($"Host=localhost");
    }

    private class SqlServerTestResource : IResourceWithConnectionString
    {
        public string Name => "sqlserver-test";
        public ResourceAnnotationCollection Annotations { get; } = new();
        public ReferenceExpression ConnectionStringExpression =>
            ReferenceExpression.Create($"Server=localhost");
    }

    private class MySqlTestResource : IResourceWithConnectionString
    {
        public string Name => "mysql-test";
        public ResourceAnnotationCollection Annotations { get; } = new();
        public ReferenceExpression ConnectionStringExpression =>
            ReferenceExpression.Create($"Server=localhost");
    }

    private class UnsupportedTestResource : IResourceWithConnectionString
    {
        public string Name => "unsupported-test";
        public ResourceAnnotationCollection Annotations { get; } = new();
        public ReferenceExpression ConnectionStringExpression =>
            ReferenceExpression.Create($"Server=localhost");
    }
}
