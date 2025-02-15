using Serilog;
using static WebContentCreator.Classes.GeminiAPI;
using static WebContentCreator.Classes.HtmlHelper;

namespace WebContentCreator.Classes
{
    public class HtmlGeneratorBackgroundService : BackgroundService
    {
        private readonly ILogger<HtmlGeneratorBackgroundService> _logger;
        private int _indexLingua = 0;
        private const int NumeroCaratteri = 200;

        public HtmlGeneratorBackgroundService(ILogger<HtmlGeneratorBackgroundService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string linguaSelezionata = HtmlCreator.Lingue[_indexLingua];
                _logger.LogInformation("GenerateRssItems running at: {time}", DateTimeOffset.Now);

                List<RssItem> rssItems = Rss.GenerateRssItems();
                HttpClient httpClient = new HttpClient();

                try
                {
                    foreach (var item in rssItems)
                    {
                        string customhash = GenerateObjectHash(item);
                        string sitemapPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "sitemap.xml");
                        if (SitemapContainsHash(sitemapPath, customhash)) continue;

                        string? richiesta = HtmlCreator.GeneraTestoRichiesta(item, linguaSelezionata, NumeroCaratteri);
                        if (richiesta is null) continue;

                        var geminiRequest = new GeminiRequest(richiesta);
                        string? risposta = await GetGeminiTextResponse(httpClient, geminiRequest)!;
                        if (risposta is null)
                        {
                            Log.Information("Errore nella chiamata a Gemini API ('{0}')", richiesta);
                            continue;
                        }
                        else
                        {
                            string? richiestaTitolo = HtmlCreator.GeneraTitoloRichiesta(item, linguaSelezionata);
                            var geminiTitleRequest = new GeminiRequest(richiestaTitolo!);
                            item.Title = await GetGeminiTextResponse(httpClient, geminiTitleRequest) ?? string.Empty;

                            string relativePath = Path.Combine("wwwroot", "html", DateTime.Now.ToString("yyyyMMdd"));
                            HtmlCreator.CreaFileHtml(relativePath, item, linguaSelezionata, risposta);
                        }
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Errore nel ciclo su Rss Items");
                    return;
                }
            }
        }

    }
}