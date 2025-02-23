# Random Software Principles Aggregator

A C# application that scrapes technical articles from various programming and development websites to collect interesting facts and articles. This project is meant for the Stage Three Tax in Telex integration, hosted on Render using Docker.

### Telex Integration

The scraper is designed to work as a Telex integration. When integrated with Telex:

1. The scraper automatically sends articles to your Telex channels
2. No manual API calls needed - Telex handles the webhook communication
3. Articles are posted at configured intervals

## Features

- Scrapes articles from multiple sources:
    - GeeksForGeeks (popular and recent articles)
    - MDN Web Docs (featured articles)
- Randomly selects articles from each source
- Extracts article titles, content, and URLs
- Error handling and logging for failed scraping attempts
- Summarizes articles with Groq and sends them to a Telex channel

## Project Structure

```
better-submitter/
├── Models/
│   ├── Fact.cs
│   ├── TelexWebhook.cs
│   └── TelexPayload.cs
├── Services/
│   ├── FactScraper.cs
│   └── Integration.cs
├── Properties/
│   └── launchSettings.json
├── Program.cs
├── Dockerfile
└── README.md
```

### Project Folders

- `Properties`: Contains project properties and settings.
- `Models`: Contains data models like `Fact`.
- `Services`: Contains service classes for scraping and integration.
- `Program.cs`: The startup point of the application.
## Setup Instructions

### Prerequisites
- [.NET SDK 9.0 or higher](https://dotnet.microsoft.com/download/dotnet) should be installed.
- A code editor such as [Visual Studio Code](https://code.visualstudio.com/) is recommended.

### Running Locally
```bash
      1. Clone the repository to your local machine:
         `git clone https://github.com/telexintegrations/sofware-principles-aggregator.git`
         `cd sofware-principles-aggregator`
      
      2. Restore project dependencies:
         `dotnet restore`
      
      3. Run the application locally:
         `dotnet run`
      
         This will run the API on your local machine. By default, it will be accessible at `http://localhost:5198`.
      
      4. Test the API and get the integraion by sending a GET request to the following endpoint:
         `http://localhost:5198/integration.json`
```
## API Documentation

### Endpoint: `GET /base-url/integration.json`
This endpoint returns the integration specs
Status Code 200

### Endpoint: `POST /base-url/tick`
This endpoint to be called by telex
Status Code 202

### Endpoint: `POST /base-url/webhook`
This an optional enpoint added to get data from the telex channel if need be
Status Code 200

## Implementation Details

### GeeksForGeeks Scraper

- Scrapes both popular and recent articles
- Extracts full article content
- Includes article preview and publication date
- Uses article container class selectors for reliable extraction

### MDN Scraper

- Focuses on featured articles from the MDN homepage
- Extracts article title, preview, and full content
- Handles relative URLs by converting them to absolute URLs
- Uses specific class selectors for reliable data extraction

## Deployment

The application is hosted on Render using Docker. For integration specs and the normal app, visit:

- [Integration Specs](https://sofware-principles-aggregator.onrender.com/integration.json)

## Repository

For more details, visit the [GitHub Repository](https://github.com/telexintegrations/sofware-principles-aggregator).
