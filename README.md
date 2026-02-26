# Shiny Aspire Libraries

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

---

# Shiny.Aspire.Hosting.Gluetun

Aspire hosting integration for [Gluetun](https://github.com/qdm12/gluetun), a lightweight VPN client container supporting multiple providers. Models Gluetun as a first-class Aspire resource and lets other containers route their traffic through the VPN tunnel.

## Package

| Package | NuGet | Usage |
|---|---|---|
| `Shiny.Aspire.Hosting.Gluetun` | [![NuGet](https://img.shields.io/nuget/v/Shiny.Aspire.Hosting.Gluetun.svg)](https://www.nuget.org/packages/Shiny.Aspire.Hosting.Gluetun) | Aspire AppHost -- adds a Gluetun VPN container and routes other containers through it |

## Quick Start

Install `Shiny.Aspire.Hosting.Gluetun` in your AppHost project.

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var vpn = builder.AddGluetun("vpn")
    .WithVpnProvider("mullvad")
    .WithWireGuard(builder.AddParameter("wireguard-key", secret: true))
    .WithServerCountries("US", "Canada");

var scraper = builder.AddContainer("scraper", "my-scraper")
    .WithHttpEndpoint(targetPort: 8080);

vpn.WithRoutedContainer(scraper);

builder.Build().Run();
```

This creates a Gluetun VPN container with Mullvad WireGuard, then routes the `scraper` container's traffic through it. At runtime the scraper joins the Gluetun network namespace (`--network container:vpn`). On Docker Compose publish, routed containers get `network_mode: "service:vpn"` and their ports are transferred to the Gluetun service.

## API Reference

### AddGluetun

Creates a Gluetun container resource with `NET_ADMIN` capability and `/dev/net/tun` device access.

```csharp
IResourceBuilder<GluetunResource> AddGluetun(
    this IDistributedApplicationBuilder builder,
    string name,
    int? httpProxyPort = null,
    int? shadowsocksPort = null)
```

The optional port parameters expose Gluetun's built-in HTTP proxy (default target 8888) and Shadowsocks proxy (default target 8388) endpoints.

### VPN Provider Configuration

```csharp
// Set the VPN service provider (required)
vpn.WithVpnProvider("mullvad");

// OpenVPN -- string credentials
vpn.WithOpenVpn("username", "password");

// OpenVPN -- Aspire parameter resources (recommended for secrets)
vpn.WithOpenVpn(
    builder.AddParameter("openvpn-user"),
    builder.AddParameter("openvpn-pass", secret: true));

// WireGuard -- string key
vpn.WithWireGuard("my-private-key");

// WireGuard -- Aspire parameter resource (recommended for secrets)
vpn.WithWireGuard(builder.AddParameter("wireguard-key", secret: true));
```

### Server Selection

```csharp
vpn.WithServerCountries("US", "Canada", "Germany");
vpn.WithServerCities("New York", "Toronto");
```

Values are comma-joined and set as `SERVER_COUNTRIES` / `SERVER_CITIES` environment variables.

### Proxy Features

```csharp
vpn.WithHttpProxy();           // enables Gluetun's built-in HTTP proxy (HTTPPROXY=on)
vpn.WithHttpProxy(false);      // disables it (HTTPPROXY=off)
vpn.WithShadowsocks();         // enables Shadowsocks proxy (SHADOWSOCKS=on)
vpn.WithShadowsocks(false);    // disables it (SHADOWSOCKS=off)
```

### Network & Firewall

```csharp
vpn.WithFirewallOutboundSubnets("10.0.0.0/8", "192.168.0.0/16");
vpn.WithTimezone("America/New_York");
```

### Generic Environment Variables

Pass any Gluetun environment variable not covered by the typed methods:

```csharp
vpn.WithGluetunEnvironment("DNS_ADDRESS", "1.1.1.1");
vpn.WithGluetunEnvironment("UPDATER_PERIOD", builder.AddParameter("updater-period"));
```

### Routing Containers Through the VPN

```csharp
vpn.WithRoutedContainer(scraper);
vpn.WithRoutedContainer(downloader);
```

Each call:
1. Adds a `GluetunRoutedResourceAnnotation` to the Gluetun resource
2. Sets `--network container:<vpn-name>` runtime args on the routed container
3. On Docker Compose publish, sets `network_mode: "service:<vpn-name>"` on the routed container and transfers its port mappings to the Gluetun service

You can route multiple containers through the same VPN.

## Docker Compose Publish

When you publish with `dotnet run --publisher manifest` or Docker Compose, routed containers automatically get:

```yaml
services:
  vpn:
    image: qmcgaw/gluetun:latest
    cap_add:
      - NET_ADMIN
    devices:
      - /dev/net/tun
    environment:
      - VPN_SERVICE_PROVIDER=mullvad
      - VPN_TYPE=wireguard
      - WIREGUARD_PRIVATE_KEY=${wireguard-key}
      - SERVER_COUNTRIES=US,Canada
    ports:
      - "8080:8080"    # forwarded from scraper
  scraper:
    image: my-scraper
    network_mode: "service:vpn"
    # ports moved to vpn service
```

## Supported VPN Providers

Gluetun supports 30+ VPN providers. See the [Gluetun wiki](https://github.com/qdm12/gluetun-wiki) for the full list and provider-specific environment variables. Use `WithGluetunEnvironment` for any provider-specific settings not covered by the typed methods.

## Requirements

- .NET 10
- .NET Aspire 13.1+
- Microsoft Orleans 10.0+ (for Orleans packages only)
