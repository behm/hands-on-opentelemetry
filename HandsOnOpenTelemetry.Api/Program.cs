using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry()
    //.ConfigureResource(resource => resource.AddService("dotnet-frontend"))
    .ConfigureResource(resourceBuilder => 
        resourceBuilder
            .AddService("dotnet-frontend")
            .AddAttributes(new List<KeyValuePair<string, object>>
            {
                new("deployment-environment", "myLaptop")
            })
    )
    .WithTracing(tpb =>
        tpb
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter()
            .AddOtlpExporter())
    ;

builder.Logging.AddOpenTelemetry(options => options.AddOtlpExporter());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/hello", (string firstName, string lastName, [FromServices] ILogger logger) =>
{
    logger.LogInformation("logging hello: firstName={firstName} lastName={lastName}", firstName, lastName);

    Activity.Current?.SetTag("FirstName", lastName);
    Activity.Current?.SetTag("Surname", lastName);

    return $"Hello, {firstName}!";
});

//app.MapGet("/weatherforecast", () =>
//{
//    var forecast =  Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();
//    return forecast;
//})
//.WithName("GetWeatherForecast")
//.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
