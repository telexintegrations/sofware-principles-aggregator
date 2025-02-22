using System.Text.Json;
using DevFactsAgregatorIntegration.Models;
using DevFactsAgregatorIntegration.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddCors();

builder.Services.AddSingleton<FactScrapper>();

var baseUrl = builder.Configuration["BaseUrl"];

var app = builder.Build();

app.UseCors((options) => options.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());


app.MapGet("/", () => $"Hello World!,{baseUrl}");

app.MapPost("/webhook", async (HttpRequest request) =>
{
    Console.WriteLine($"this enpoint was triggerd");
    Console.WriteLine(await request.ReadFromJsonAsync<object>());
});


app.MapPost("/tick", async (HttpRequest request, IHttpClientFactory httpClient, FactScrapper factScrapper) =>
{
    Console.WriteLine("Tick endpoint triggered - Starting background task...");
    var payload = await request.ReadFromJsonAsync<TelexPayloadModel>();
    if (payload == null)
        return Results.BadRequest("Invalid JSON");

    string channelId = payload.ChannelId;
    string returnUrl = payload.ReturnUrl;

    Console.WriteLine($"Tick Received: {channelId}, {returnUrl}");
    _ = Task.Run(async () =>
    {
        try
        {
            var client = httpClient.CreateClient();

            var fact = await factScrapper.ScrapRandomFact();

            var message = $@"
Title: {fact.Tittle}

Summary: {fact.Content}

Source: {fact.Source}
    ";

            var testing = new TelexWebhookModel()
            {
                Username = "Tech Fact Aggregator",
                Status = "success",
                Message = message,
                EventName = "Random Tech Fact"
            };

            var response = await client.PostAsJsonAsync(returnUrl, testing);
            Console.WriteLine($"Data posted to telex channel - Status: {response.StatusCode}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in background task: {e}");
        }
    });

    return Results.Accepted("", new { Status = "processing" });
});


app.MapGet("/integration.json", () => { return Results.Ok(Integration.GetIntegrationSpecs(baseUrl)); });

app.Run();