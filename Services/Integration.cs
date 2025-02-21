
namespace DevFactsAgregatorIntegration.Services;

public class Integration
{
    public static object GetIntegrationSpecs(string baseUrl)
    {
        var specs = new
        {
            data = new
            {
                date = new { created_at = "2025-02-20", updated_at = "2025-02-20" },
                descriptions = new
                {
                    app_description = "An app for scrapping and summarizing random software design principles",
                    app_logo = "https://res.cloudinary.com/dscs0driw/image/upload/v1740095705/tech-facts_s8r6zd.jpg",
                    app_name = "Software Development Principles Aggregator",
                    app_url = baseUrl,
                    background_color = "#fff"
                },
                integration_category = "Communication & Collaboration",
                author = "Gboun Marvellous",
                website = baseUrl,
                integration_type = "interval",
                is_active = true,
                key_features = new[]
                {
                    "Scrapping",
                    "Scrape random tech information websites",
                    "Get random software development websites",
                    "Summarize and send them to designated telex channel at intervals"
                },
                settings = new[]
                {
                    new { label = "interval", type = "text", required = true, default_value = "* * * * *" },
                    new
                    {
                        label = "Tech Facts", type = "text", required = true,
                        default_value = "Some Random Tech Design Principle"
                    }
                },
                tick_url = $"{baseUrl}/tick",
                target_url = ""
            }
        };
        return specs;
    }
}





