using Shiny.Aspire.Orleans.Server;

var builder = WebApplication.CreateBuilder(args);

builder.UseOrleansWithAdoNet();

var app = builder.Build();

app.MapGet("/", () => "Orleans Silo is running");

app.Run();
