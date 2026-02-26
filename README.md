# Shiny.Aspire.Orleans

Zero-friction integration between [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) and [Microsoft Orleans](https://learn.microsoft.com/dotnet/orleans/) for ADO.NET storage backends. Automatically provisions Orleans database schemas and wires up clustering, grain persistence, and reminders from Aspire configuration -- no manual SQL scripts or connection string plumbing required.

## Supported Databases

- PostgreSQL
- SQL Server
- MySQL

## Packages

| Package | NuGet | Usage |
|---|---|---|
| `Shiny.Aspire.Orleans.Hosting` | [![NuGet](https://img.shields.io/nuget/v/Shiny.Aspire.Orleans.Hosting.svg)](https://www.nuget.org/packages/Shiny.Aspire.Orleans.Hosting) | Aspire AppHost -- auto-runs Orleans schema scripts when the database becomes ready |
| `Shiny.Aspire.Orleans.Server` | [![NuGet](https://img.shields.io/nuget/v/Shiny.Aspire.Orleans.Server.svg)](https://www.nuget.org/packages/Shiny.Aspire.Orleans.Server) | Orleans silo -- configures ADO.NET clustering, grain storage, and reminders from Aspire config |
| `Shiny.Aspire.Orleans.Client` | [![NuGet](https://img.shields.io/nuget/v/Shiny.Aspire.Orleans.Client.svg)](https://www.nuget.org/packages/Shiny.Aspire.Orleans.Client) | Orleans client -- configures ADO.NET clustering from Aspire config |

## Quick Start

### 1. AppHost (Aspire Orchestrator)

Install `Shiny.Aspire.Orleans.Hosting` in your AppHost project.

```csharp
using Shiny.Aspire.Orleans.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("pg")
    .AddDatabase("orleans-db");

var orleans = builder.AddOrleans("cluster")
    .WithClustering(db)
    .WithGrainStorage("Default", db)
    .WithReminders(db)
    .WithDatabaseSetup(db); // <-- creates all Orleans tables automatically

builder.AddProject<Projects.MySilo>("silo")
    .WithReference(orleans)
    .WaitFor(db);

builder.AddProject<Projects.MyApi>("api")
    .WithReference(orleans.AsClient())
    .WaitFor(db);

builder.Build().Run();
```

`WithDatabaseSetup` subscribes to Aspire's `ResourceReadyEvent` for the database resource. When the database container is up and accepting connections, it automatically executes the Orleans SQL schema scripts (clustering tables, persistence tables, reminders tables, stored procedures, and query registrations).

### 2. Orleans Silo

Install `Shiny.Aspire.Orleans.Server` in your silo project.

```csharp
using Shiny.Aspire.Orleans.Server;

var builder = WebApplication.CreateBuilder(args);
builder.UseOrleansWithAdoNet(); // reads Aspire-injected config automatically
var app = builder.Build();
app.Run();
```

`UseOrleansWithAdoNet()` reads the `Orleans:Clustering`, `Orleans:GrainStorage`, and `Orleans:Reminders` configuration sections that Aspire injects automatically, and configures the appropriate ADO.NET providers with the correct connection strings and invariants.

### 3. Orleans Client

Install `Shiny.Aspire.Orleans.Client` in your client project (e.g. an API gateway).

```csharp
using Shiny.Aspire.Orleans.Client;

var builder = WebApplication.CreateBuilder(args);
builder.UseOrleansClientWithAdoNet(); // reads Aspire-injected config automatically
var app = builder.Build();

app.MapGet("/counter/{name}", async (string name, IClusterClient client) =>
{
    var grain = client.GetGrain<ICounterGrain>(name);
    var count = await grain.GetCount();
    return Results.Ok(new { name, count });
});

app.Run();
```

## Feature Selection

By default, `WithDatabaseSetup` creates schemas for all Orleans features. You can limit this using the `OrleansFeature` flags enum:

```csharp
// Only set up clustering and persistence tables (no reminders)
orleans.WithDatabaseSetup(db, OrleansFeature.Clustering | OrleansFeature.Persistence);

// Only set up clustering
orleans.WithDatabaseSetup(db, OrleansFeature.Clustering);
```

Available flags:

| Flag | Value | Description |
|---|---|---|
| `Clustering` | 1 | Membership tables for silo discovery |
| `Persistence` | 2 | Grain storage tables |
| `Reminders` | 4 | Reminder tables |
| `All` | 7 | All of the above (default) |

## Using Different Databases

The database type is auto-detected from the Aspire resource. Just swap the resource builder:

```csharp
// PostgreSQL
var db = builder.AddPostgres("pg").AddDatabase("orleans-db");

// SQL Server
var db = builder.AddSqlServer("sql").AddDatabase("orleans-db");

// MySQL
var db = builder.AddMySql("mysql").AddDatabase("orleans-db");
```

Everything else stays the same -- the correct SQL scripts, connection provider, and ADO.NET invariant are selected automatically.

## How It Works

### Configuration Flow

Aspire automatically injects configuration into your silo and client projects when you use `.WithReference(orleans)`. The injected configuration looks like:

```
Orleans:Clustering:ProviderType = "PostgresDatabase"
Orleans:Clustering:ServiceKey   = "orleans-db"
Orleans:GrainStorage:Default:ProviderType = "PostgresDatabase"
Orleans:GrainStorage:Default:ServiceKey   = "orleans-db"
Orleans:Reminders:ProviderType  = "PostgresDatabase"
Orleans:Reminders:ServiceKey    = "orleans-db"
ConnectionStrings:orleans-db    = "Host=...;Database=..."
```

The `UseOrleansWithAdoNet()` and `UseOrleansClientWithAdoNet()` extension methods read these sections and configure Orleans with the matching ADO.NET providers (`Npgsql`, `Microsoft.Data.SqlClient`, or `MySqlConnector`).

### Schema Provisioning

`WithDatabaseSetup` runs embedded SQL scripts in order:

1. **Main** -- creates the `OrleansQuery` table (Orleans' query registry)
2. **Clustering** -- creates `OrleansMembershipVersionTable`, `OrleansMembershipTable`, and related stored procedures/functions
3. **Persistence** -- creates the `OrleansStorage` table and related stored procedures/functions
4. **Reminders** -- creates `OrleansRemindersTable` and related stored procedures/functions

Scripts are executed when Aspire raises the `ResourceReadyEvent` for the database, ensuring the database is accepting connections before any schema setup runs.

## Multiple Grain Storage Providers

The server package supports multiple named grain storage providers:

```csharp
// AppHost
var orleans = builder.AddOrleans("cluster")
    .WithClustering(db)
    .WithGrainStorage("Default", db)
    .WithGrainStorage("Archive", archiveDb)
    .WithDatabaseSetup(db);

// Grain
public class MyGrain(
    [PersistentState("state", "Default")] IPersistentState<MyState> state,
    [PersistentState("archive", "Archive")] IPersistentState<ArchiveState> archive
) : Grain, IMyGrain { }
```

## Sample

The `samples/` directory contains a complete working example:

| Project | Description |
|---|---|
| `Sample.AppHost` | Aspire orchestrator wiring PostgreSQL, Orleans silo, and API |
| `Sample.Silo` | Orleans silo host |
| `Sample.Api` | HTTP API that calls grains via `IClusterClient` |
| `Sample.GrainInterfaces` | `ICounterGrain` interface |
| `Sample.Grains` | `CounterGrain` with persistent state |

Run the sample:

```bash
dotnet run --project samples/Sample.AppHost
```

## Requirements

- .NET 10
- .NET Aspire 13.1+
- Microsoft Orleans 10.0+
