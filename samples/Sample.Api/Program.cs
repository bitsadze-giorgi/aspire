using Sample.GrainInterfaces;
using Shiny.Aspire.Orleans.Client;

var builder = WebApplication.CreateBuilder(args);

builder.UseOrleansClientWithAdoNet();

var app = builder.Build();

app.MapGet("/", () => "Orleans API Client is running");

app.MapGet("/counter/{name}", async (string name, IClusterClient client) =>
{
    var grain = client.GetGrain<ICounterGrain>(name);
    var count = await grain.GetCount();
    return Results.Ok(new { name, count });
});

app.MapPost("/counter/{name}/increment", async (string name, IClusterClient client) =>
{
    var grain = client.GetGrain<ICounterGrain>(name);
    var count = await grain.Increment();
    return Results.Ok(new { name, count });
});

app.Run();
