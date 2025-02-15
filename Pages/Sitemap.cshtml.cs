using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Xml.Linq;

namespace WebContentCreator
{
    public class SitemapModel : PageModel
    {
        private readonly IWebHostEnvironment _env;

        public SitemapModel(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult OnGet()
        {
            var urls = GetHtmlPagesUrls(); // Recupera gli URL delle pagine HTML

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

            return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
        }

        private List<string> GetHtmlPagesUrls()
        {
            var urls = new List<string>();
            string baseUrl = $"{Request.Scheme}://{Request.Host}/html/"; // Base URL del sito
            string htmlRootPath = Path.Combine(_env.WebRootPath, "html"); // Percorso wwwroot/html/

            if (Directory.Exists(htmlRootPath))
            {
                // Scansiona le cartelle con le date
                var dateDirectories = Directory.GetDirectories(htmlRootPath);

                foreach (var dateDir in dateDirectories)
                {
                    string dateFolderName = Path.GetFileName(dateDir); // Estrarre il nome della cartella
                    var files = Directory.GetFiles(dateDir, "*.html"); // Cerca i file .html

                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        string pageUrl = $"{baseUrl}{dateFolderName}/{fileName}";
                        urls.Add(pageUrl);
                    }
                }
            }

            return urls;
        }
    }
}