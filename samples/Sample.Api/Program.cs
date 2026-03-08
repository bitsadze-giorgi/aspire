using Sample.GrainInterfaces;
using Shiny.Aspire.Orleans.Client;

var builder = WebApplication.CreateBuilder(args);

builder.UseOrleansClient(silo => silo.UseAdoNetClient());

var app = builder.Build();

app.MapGet("/", () => "Orleans API Client is running");

// Counter grain endpoints
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

// Reminder grain endpoints
app.MapPost("/reminder/{name}/start", async (string name, IClusterClient client) =>
{
    var grain = client.GetGrain<IReminderGrain>(name);
    await grain.Start(TimeSpan.FromMinutes(1));
    return Results.Ok(new { name, status = "started" });
});

app.MapPost("/reminder/{name}/stop", async (string name, IClusterClient client) =>
{
    var grain = client.GetGrain<IReminderGrain>(name);
    await grain.Stop();
    return Results.Ok(new { name, status = "stopped" });
});

app.MapGet("/reminder/{name}", async (string name, IClusterClient client) =>
{
    var grain = client.GetGrain<IReminderGrain>(name);
    var lastTick = await grain.GetLastTick();
    return Results.Ok(new { name, lastTick });
});

app.Run();
