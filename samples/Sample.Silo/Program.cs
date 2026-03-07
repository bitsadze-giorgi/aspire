using Orleans.Dashboard;
using Shiny.Aspire.Orleans.Server;

var builder = WebApplication.CreateBuilder(args);

builder.UseOrleans(silo =>
{
    silo.UseAdoNet();
    silo.AddDashboard();
});

var app = builder.Build();

app.MapGet("/", () => "Orleans Silo is running");
app.MapOrleansDashboard();

app.Run();
