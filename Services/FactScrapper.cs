using System.Runtime.CompilerServices;
using DevFactsAgregatorIntegration.Models;
using HtmlAgilityPack;
using OpenAI.Chat;

namespace DevFactsAgregatorIntegration.Services;

public class FactScrapper(IConfiguration configuration, IHttpClientFactory httpClientFactory)
{
    private static readonly Random Random = new();
    private readonly string GeekForGeekUrl = configuration["GeekForGeekUrl"] ?? throw new ArgumentNullException("GeekForGeekUrl is missing in config");
    private readonly string MediumUrl = configuration["MediumUrl"] ?? throw new ArgumentNullException("MediumUrl is missing in config");
    private readonly string MdnUrl = configuration["MdnUrl"] ?? throw new ArgumentNullException("MdnUrl is missing in config");
    private readonly string OpenAiKey= configuration["OpenAiKey"] ?? throw new ArgumentNullException("OpenAiKey is Missiing in config");

    public async Task<Fact> ScrapRandomFact()
    {
        var scrappingMethods = new List<Func<Task<Fact>>>()
        {
            GetRandomGeekforGeekFact,
            GetRandomMDNFact,
            GetRandomMediumFact
        };

        // var randomScrappingMethod = scrappingMethods[Random.Next(scrappingMethods.Count)];
        return await GetRandomGeekforGeekFact();
    }

    private  async Task<Fact> GetRandomGeekforGeekFact()
    {
    try
    {
            var articleTypes = new[] { "popular", "recent" };
            var selectedType = articleTypes[Random.Next(articleTypes.Length)];
            
            var baseUrl = $"{GeekForGeekUrl}/?type={selectedType}";
            Console.WriteLine($"Fetching {selectedType} articles from GeeksForGeeks");
            
            var doc = await LoadHtmlDocument(baseUrl);
            
            
            var articleNodes = doc.DocumentNode.SelectNodes("//div[starts-with(@class, 'article_container')]");
            
            if (articleNodes == null || !articleNodes.Any()) 
            {
                Console.WriteLine("No articles found on GeeksForGeeks");
                return null;
            }

            Console.WriteLine($"Found {articleNodes.Count} {selectedType} articles");

            var randomArticle = articleNodes[Random.Next(articleNodes.Count)];
            Console.WriteLine($"Selected container class: {randomArticle.GetAttributeValue("class", "")}");
            
            var articleUrl = randomArticle
                .SelectSingleNode(".//div[@class='article_subheading']/a")
                ?.GetAttributeValue("href", "");

            if (string.IsNullOrEmpty(articleUrl))
            {
                Console.WriteLine("Could not find article URL");
                return null;
            }
            
            var title = randomArticle
                .SelectSingleNode(".//div[@class='article_subheading']/a")
                ?.GetAttributeValue("title", "")
                ?.Trim();

            var preview = randomArticle
                .SelectSingleNode(".//span[@class='article_content']")
                ?.InnerText.Trim();

            var date = randomArticle
                .SelectSingleNode(".//div[@class='article_date']")
                ?.InnerText.Trim();

            // Get the full article content
            var articleDoc = await LoadHtmlDocument(articleUrl);
            var content = articleDoc.DocumentNode.SelectSingleNode("//article")?.InnerText.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                Console.WriteLine("Could not find article title or content");
                return null;
            }

            var summary = await GenerateSummary(content);

            return new Fact()
            {
                Tittle = title,
                Content = summary,
                Source = $"GeeksForGeeks ({selectedType})",
                Url = articleUrl
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scraping GeeksForGeeks: {ex.Message}");
            return null;
        }
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
    
    private  async Task<string> GenerateSummary(string content)
    {
        var client = new ChatClient(model: "gpt-4", apiKey: OpenAiKey);
        
        var completion = await client.CompleteChatAsync($" You are a helpful assistant that creates engaging summaries of technical articles. Include one interesting fun fact from the article if possible.,Please summarize this technical article: {content}. The summary must be 2-3 lines and not more than 50 words");

        return completion.Value.Content[0].Text;
    }
}
