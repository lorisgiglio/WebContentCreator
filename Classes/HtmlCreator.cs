using Serilog;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WebContentCreator.Classes
{
    public class HtmlCreator
    {
        public static string[] Lingue = new string[]
        {
/*            "Cinese mandarino",
            "Inglese",
            "Spagnolo",
            "Arabo",
            "Russo",
            "Giapponese",
            "Tedesco",
            "Francese",*/
            "it"
        };

        public static void CreaFileHtml(string relativePath, RssItem argomento, string lingua, string contenuto)
        {
            string customhash = GenerateObjectHash(argomento);
            string sitemapPath = Path.Combine(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "sitemap.xml");
            if (SitemapContainsHash(sitemapPath, customhash)) return;

            string dataOggi = DateTime.Now.ToString("yyyyMMdd");
            string dataPubblicazione = DateTime.Now.ToString();

            if (DateTime.TryParse(argomento.PubDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime data))
            {
                dataPubblicazione = data.ToString("dd-MM-yyyy HH:mm");
            }

            string arg = SanitizeFileName(argomento.Title);
            string topic = SanitizeFileName(argomento.Topic).ToCamelCase();
            string nomeFile = $"{lingua}_{topic}_{arg}_{customhash}.html";

            contenuto = CleanHtml(contenuto);

            string htmlContent = $@"
    <div class='container mt-5'>
        <h1 class='display-4 text-center'>{argomento.Title}</h1>
        <p class='lead text-justify'>{contenuto}</p><p>(<datapub>{dataPubblicazione}</datapub>)</p></div>";
            //        " <a class='text-center' target='blank' href='{argomento.Link}'>Fonte</a>"+


            string AbsolutePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath); //, lingua
            if (!Directory.Exists(AbsolutePath))
            {
                Log.Information("Creazione directory {0}", AbsolutePath);
                Directory.CreateDirectory(AbsolutePath);
            }
            string FullPath = Path.Combine(AbsolutePath, nomeFile);

            try
            {
                File.WriteAllText(FullPath, htmlContent);
            }
            catch
            {
                Log.Error("Errore nella creazione del file {0}", FullPath);
            }

            Log.Information($"File {nomeFile} creato con successo.");
        }
        public static string? GeneraTestoRichiesta(RssItem argomento, string lingua, int numeroCaratteri)
        {
            if (argomento is null)
            {
                return null!;
            }

            const string base_request = "Scrivi un articolo nella lingua associata al codice '{0}' come se fossi un giornalista su '{1} - {2}'. " +
                                        "Dai un titolo e scrivi almeno {3} caratteri in formato HTML con grassetto, corsivo e altra formattazione necessaria ed eventuali link (reali, non fittizi)." +
                                        "Evita testi dove non puoi citare le persone coinvolte e il testo deve essere originale in modo da non incorrere in violazioni di copyright." +
                                        "Sostituisci i ritorni a capo con i tag BR, non includere tag html-head-body.";

            return string.Format(base_request, lingua, argomento.Description, argomento.Title, numeroCaratteri, argomento.Link, argomento.PubDate);
        }
        public static string? GeneraTitoloRichiesta(RssItem item, string lingua)
        {
            if (string.IsNullOrWhiteSpace(item.Title))
            {
                return null!;
            }

            const string base_request = "Riformula questo titolo '{0}' nella lingua associata al codice '{1}' in modo da renderlo diverso dall'originale. Non voglio alternative, solo un titolo.";

            return string.Format(base_request, item.Title, lingua);
        }
        static string SanitizeFileName(string input)
        {
            string sanitized = Regex.Replace(input, "[\\/:*?\"<>|]", "");
            sanitized = Regex.Replace(sanitized, "[\\s,]+", "-");

            if (sanitized.Length > 100)
                sanitized = sanitized.Substring(0, 100);

            sanitized = sanitized.Trim('-');

            return sanitized.Replace("'", string.Empty)
                .Replace(";", string.Empty)
                .Replace(":", string.Empty)
                .Replace("%", string.Empty)
                .Replace("+", string.Empty);
        }
        static string CleanHtml(string htmlContent)
        {
            string pattern = @"<a\s+href=""https:\/\/www\.example\.com\/[^""]+\"">.*?sostituire con un link reale.*?<\/a>";
            return Regex.Replace(htmlContent, pattern, "", RegexOptions.IgnoreCase);
        }
        public static string GenerateObjectHash<T>(T obj)
        {
            string json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(json);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                string hash = Convert.ToBase64String(hashBytes)
                                    .Replace("+", "-")
                                    .Replace("/", "_")
                                    .TrimEnd('=');

                return hash.Length > 30 ? hash.Substring(0, 30) : hash;
            }
        }
        static bool SitemapContainsHash(string sitemapPath, string identifier)
        {
            if (!File.Exists(sitemapPath))
            {
                return false;
            }

            XDocument doc = XDocument.Load(sitemapPath);

            XNamespace ns = doc.Root?.GetDefaultNamespace() ?? "";
            List<string> htmlFiles = doc.Descendants(ns + "url")
                .Select(e => e.Element(ns + "loc")?.Value)
                .Where(value => !string.IsNullOrEmpty(value))
                .Select(Path.GetFileName)
                .Where(name => name?.EndsWith(".html", StringComparison.OrdinalIgnoreCase) == true)
                .ToList()!;

            if (htmlFiles is null || htmlFiles.Count == 0) return false;
            return htmlFiles.Any(file => file.Contains(identifier));
        }
    }

}
