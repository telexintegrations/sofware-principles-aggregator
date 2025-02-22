using System.Text.Json;
using DevFactsAgregatorIntegration.Models;
using DevFactsAgregatorIntegration.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddCors();

var baseUrl = builder.Configuration["BaseUrl"];

var app = builder.Build();

app.UseCors((options) => options.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());


app.MapGet("/", () => $"Hello World!,{baseUrl}");

app.MapPost("/webhook", async (HttpRequest request) =>
{
    Console.WriteLine($"this enpoint was triggerd");
    Console.WriteLine(await request.ReadFromJsonAsync<object>());
});


app.MapPost("/tick", (HttpRequest request, IHttpClientFactory httpClient) =>
{
    Console.WriteLine("Tick endpoint triggered - Starting background task...");
    _ = Task.Run(async () =>
    {
        try
        {
            var client = httpClient.CreateClient();
            
            var payload = await request.ReadFromJsonAsync<TelexPayloadModel>();
            if (payload == null)
            {
                Console.WriteLine("Invalid JSON payload");;
                return;
            }

            string channelId = payload.ChannelId;
            string returnUrl = payload.ReturnUrl;

            Console.WriteLine($"Tick Received - Channel ID: {channelId}, Return URL: {returnUrl}");

            var testing = new TelexWebhookModel()
            {
                Username = "Tech Fact Aggregator",
                Status = "success",
                Message = "Tech Fact is good",
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

    return Results.Accepted("",new { Status = "processing"});
});


// app.MapPost("/tick",  async (HttpRequest request, IHttpClientFactory httpClient) =>
// {
//     try
//     {
//         var client = httpClient.CreateClient();
//         var payload = await request.ReadFromJsonAsync<TelexPayloadModel>();
//         if (payload == null)
//             return Results.BadRequest("Invalid JSON");
//
//         string channelId = payload.ChannelId;
//         string returnUrl = payload.ReturnUrl;
//
//         Console.WriteLine($"Tick Received: {channelId}, {returnUrl}");
//
//         var testing = new TelexWebhookModel()
//         {
//             Username = "Tech Fact Aggregator",
//             Status = "success",
//             Message = "Tech Fact is good",
//             EventName = "Random Tech Fact"
//         };
//
//         var data= await client.PostAsJsonAsync(returnUrl, testing);
//         
//         Console.WriteLine($"Data posted to telex chanel: {data.RequestMessage}, {data.ReasonPhrase}");
//
//         return Results.Accepted("",new { Status = "success", Data= data.ReasonPhrase });
//     }
//     catch (Exception e)
//     {
//         Console.WriteLine(e);
//         throw;
//     }
//     
// });

app.MapGet("/integration.json", () =>
{
    return Results.Ok(Integration.GetIntegrationSpecs(baseUrl));
});

app.Run();
