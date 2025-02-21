using System.Runtime.CompilerServices;
using DevFactsAgregatorIntegration.Models;
using HtmlAgilityPack;

namespace DevFactsAgregatorIntegration.Services;

public class FactScrapper(IConfiguration configuration, IHttpClientFactory httpClientFactory)
{
    private static readonly Random Random = new();
    private readonly string GeekForGeekUrl = configuration["GeekForGeekUrl"] ?? throw new ArgumentNullException("GeekForGeekUrl is missing in config");
    private readonly string MediumUrl = configuration["MediumUrl"] ?? throw new ArgumentNullException("MediumUrl is missing in config");
    private readonly string MdnUrl = configuration["MdnUrl"] ?? throw new ArgumentNullException("MdnUrl is missing in config");

    public async Task<Fact> ScrapRandomFact()
    {
        var scrappingMethods = new List<Func<Task<Fact>>>()
        {
            GetRandomGeekforGeekFact,
            GetRandomMDNFact,
            GetRandomMediumFact
        };

        var randomScrappingMethod = scrappingMethods[Random.Next(scrappingMethods.Count)];
        return await randomScrappingMethod();
    }

    private async Task<Fact> GetRandomGeekforGeekFact()
    {
        var document = await LoadHtmlDocument(GeekForGeekUrl);
        
        
        return new Fact()
        {
            Tittle = "Testing Fact",
            Content = "Test Content",
            Source = "GeekForGeeks",
            Url = GeekForGeekUrl
        };
    }

    private async Task<Fact> GetRandomMDNFact()
    {
        var document = await LoadHtmlDocument(MdnUrl);
        return new Fact()
        {
            Tittle = "Testing Fact",
            Content = "Test Content",
            Source = "MDN",
            Url = MdnUrl
        };
    }

    private async Task<Fact> GetRandomMediumFact()
    {
        var document = await LoadHtmlDocument(MediumUrl);
        return new Fact()
        {
            Tittle = "Testing Fact",
            Content = "Test Content",
            Source = "Medium",
            Url = MediumUrl
        };
    }

    private async Task<HtmlDocument> LoadHtmlDocument(string url)
    {
        using var client = httpClientFactory.CreateClient();
        var html = await client.GetStringAsync(url);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }
}
