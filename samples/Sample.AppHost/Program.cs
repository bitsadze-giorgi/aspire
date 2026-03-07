using Shiny.Aspire.Orleans.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// --- Parameters for VPN credentials (optional, for Gluetun demo) ---
var vpnUser = builder.AddParameter("vpn-user", secret: true);
var vpnPassword = builder.AddParameter("vpn-password", secret: true);

// --- Database ---
var db = builder.AddPostgres("pg")
    .WithPgAdmin()
    .AddDatabase("orleans-db");

// --- Orleans cluster (clustering, persistence, reminders all on Postgres) ---
var orleans = builder.AddOrleans("cluster")
    .WithClustering(db)
    .WithGrainStorage("Default", db)
    .WithReminders(db)
    .WithDatabaseSetup(db);

// --- Silo (full server with dashboard) ---
builder.AddProject<Projects.Sample_Silo>("silo")
    .WithReference(orleans)
    .WithHttpsEndpoint(port: 8080, name: "dashboard")
    .WaitFor(db);

// --- API (client) ---
builder.AddProject<Projects.Sample_Api>("api")
    .WithReference(orleans.AsClient())
    .WaitFor(db);

// --- Gluetun VPN container (demo) ---
var gluetun = builder.AddGluetun("vpn")
    .WithVpnProvider("nordvpn")
    .WithOpenVpn(vpnUser, vpnPassword)
    .WithServerCountries("United States")
    .WithHttpProxy();

builder.Build().Run();
