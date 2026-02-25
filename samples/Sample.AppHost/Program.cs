using Shiny.Aspire.Orleans.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("pg")
    .AddDatabase("orleans-db");

var orleans = builder.AddOrleans("cluster")
    .WithClustering(db)
    .WithGrainStorage("Default", db)
    .WithReminders(db)
    .WithDatabaseSetup(db);

builder.AddProject<Projects.Sample_Silo>("silo")
    .WithReference(orleans)
    .WaitFor(db);

builder.AddProject<Projects.Sample_Api>("api")
    .WithReference(orleans.AsClient())
    .WaitFor(db);

builder.Build().Run();
