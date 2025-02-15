using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WebContentCreator.Classes
{
    public static class HtmlHelper
    {
        public static string HtmlDecode(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return WebUtility.HtmlDecode(input);
        }
        public static string RemoveHtmlTags(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            string noHtml = Regex.Replace(input, "<.*?>", string.Empty);
            noHtml = Regex.Replace(noHtml, @"\s+", " ").Trim();

            return noHtml;
        }
        public static string RemoveExampleLinks(this string input)
        {
            string pattern = "<a[^>]*?href=[\"']?(https?:\\/\\/)?(www\\.)?(example\\.(com|it))[\"']?[^>]*?>.*?<\\/a>";
            return Regex.Replace(input, pattern, string.Empty, RegexOptions.IgnoreCase);
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
        public static bool SitemapContainsHash(string sitemapPath, string identifier)
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
