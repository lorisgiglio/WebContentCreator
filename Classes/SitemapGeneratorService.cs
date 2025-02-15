namespace WebContentCreator.Classes
{
    using System.Text;
    using System.Xml.Linq;

    public class SitemapGeneratorService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<SitemapGeneratorService> _logger;

        public SitemapGeneratorService(IWebHostEnvironment env, ILogger<SitemapGeneratorService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public void GenerateSitemap()
        {
            try
            {
                var urls = GetHtmlPagesUrls();
                XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

                var sitemap = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement(ns + "urlset",
                        urls.Select(url =>
                            new XElement(ns + "url",
                                new XElement(ns + "loc", url),
                                new XElement(ns + "lastmod", DateTime.UtcNow.ToString("yyyy-MM-dd")),
                                new XElement(ns + "changefreq", "daily"),
                                new XElement(ns + "priority", "0.8")
                            )
                        )
                    )
                );

                string wwwRootPath = _env.WebRootPath;
                if (string.IsNullOrWhiteSpace(wwwRootPath)) {
                    wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }
                string sitemapPath = Path.Combine(wwwRootPath, "sitemap.xml");
                File.WriteAllText(sitemapPath, sitemap.ToString(), Encoding.UTF8);

                _logger.LogInformation("✅ Sitemap generata con successo: {SitemapPath}", sitemapPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Errore durante la generazione della sitemap(_env.WebRootPath:{0}).", _env.WebRootPath);
            }
        }

        private List<string> GetHtmlPagesUrls()
        {
            var urls = new List<string>();
            string baseUrl = "https://www.infotoday.it/HtmlViewer/";
            string htmlRootPath = Path.Combine(_env.WebRootPath, "html");

            if (Directory.Exists(htmlRootPath))
            {
                var dateDirectories = Directory.GetDirectories(htmlRootPath);

                foreach (var dateDir in dateDirectories)
                {
                    string dateFolderName = Path.GetFileName(dateDir);
                    var files = Directory.GetFiles(dateDir, "*.html");

                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        string pageUrl = $"{baseUrl}{dateFolderName}/{fileName}";
                        urls.Add(pageUrl);
                    }
                }
            }

            urls.Add("https://www.infotoday.it/");
            return urls;
        }
    }


}
