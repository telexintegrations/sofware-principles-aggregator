using GroqSharp;
using DevFactsAgregatorIntegration.Models;
using GroqSharp.Models;
using HtmlAgilityPack;


namespace DevFactsAgregatorIntegration.Services;

public class FactScrapper(IConfiguration configuration, IHttpClientFactory httpClientFactory)
{
    private static readonly Random Random = new();
    private readonly string GeekForGeekUrl = configuration["GeekForGeekUrl"] ?? throw new ArgumentNullException("GeekForGeekUrl is missing in config");
    private readonly string MdnUrl = configuration["MdnUrl"] ?? throw new ArgumentNullException("MdnUrl is missing in config");
    private readonly string OpenAiKey= configuration["OpenAiKey"] ?? throw new ArgumentNullException("OpenAiKey is Missiing in config");

    public async Task<Fact> ScrapRandomFact()
    {
        var scrappingMethods = new List<Func<Task<Fact>>>()
        {
            GetRandomGeekforGeekFact,
            GetRandomMDNFact
        };

        var randomScrappingMethod = scrappingMethods[Random.Next(scrappingMethods.Count)];

        return await randomScrappingMethod();
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
            
            if (string.IsNullOrEmpty(summary))
            {
                summary = preview;
            }
            
            Console.WriteLine($"contnet:{content}");
            Console.WriteLine($"summary:{summary}");
            Console.WriteLine($"preview:{preview}");

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
        try
        {
            Console.WriteLine("Fetching featured articles from MDN");
        
            var document = await LoadHtmlDocument(MdnUrl);
            var articleNodes = document.DocumentNode.SelectNodes("//div[@class='tile-container']/div[@class='article-tile']");
        
            if (articleNodes == null || !articleNodes.Any())
            {
                Console.WriteLine("No articles found on MDN");
                return null;
            }

            Console.WriteLine($"Found {articleNodes.Count} featured articles");
        
            var randomArticle = articleNodes[Random.Next(articleNodes.Count)];
        
            var titleNode = randomArticle.SelectSingleNode(".//h3[@class='tile-title']/a");
            var articleUrl = titleNode?.GetAttributeValue("href", "");
            var title = titleNode?.InnerText.Trim();
            var preview = randomArticle.SelectSingleNode(".//p")?.InnerText.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(articleUrl))
            {
                Console.WriteLine("Could not find article title or URL");
                return null;
            }

            // var fullArticleUrl = new Uri(new Uri(MdnUrl), articleUrl).ToString();
            var fullArticleUrl = new Uri(new Uri(MdnUrl), articleUrl.TrimStart('/')).ToString();

            var articleDoc = await LoadHtmlDocument(fullArticleUrl);
            var contentNode = articleDoc.DocumentNode.SelectSingleNode("//div[@class='section-content']");
            var content = contentNode != null ? contentNode.InnerText.Trim() : preview;
            
            
            var summary = await GenerateSummary(content);
            
            if (string.IsNullOrEmpty(summary))
            {
                summary = content;
            }
            
            Console.WriteLine($"contnet:{content}");
            Console.WriteLine($"summary:{summary}");

            return new Fact()
            {
                Tittle = title,
                Content = summary,
                Source = "MDN Featured Articles",
                Url = fullArticleUrl
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scraping MDN: {ex.Message}");
            return null;
        }
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
            try
            {
                var apiModel = "mixtral-8x7b-32768";
                var client = new GroqClient(OpenAiKey, apiModel).SetTemperature(0.5).SetMaxTokens(512).SetTopP(1).SetStop("NONE").SetStructuredRetryPolicy(2);
                var messages = new Message
                {
                    Content =
                        $"system You are a helpful assistant that summarizes technical articles concisely. Summarize this technical article in a few sentences: {content}"
                };
                var response = await client.CreateChatCompletionAsync(messages);

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating summary: {ex.Message}");
                return null;
            }
        
    }
}
