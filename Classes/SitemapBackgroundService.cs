namespace WebContentCreator.Classes
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class SitemapBackgroundService : BackgroundService
    {
        private readonly SitemapGeneratorService _sitemapService;
        private readonly ILogger<SitemapBackgroundService> _logger;

        public SitemapBackgroundService(SitemapGeneratorService sitemapService, ILogger<SitemapBackgroundService> logger)
        {
            _sitemapService = sitemapService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("🔄 Generazione automatica della Sitemap in corso...");
                    _sitemapService.GenerateSitemap();
                    _logger.LogInformation("✅ Sitemap aggiornata con successo!");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Errore durante l'aggiornamento della Sitemap.");
                    continue;
                }

                // Aspetta 24 ore prima di aggiornare di nuovo la sitemap
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

}
