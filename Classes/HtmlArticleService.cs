using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;

namespace WebContentCreator.Classes
{
    public class HtmlArticleService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _htmlFolderPath;
        private readonly IMemoryCache _cache;

        public HtmlArticleService(IWebHostEnvironment env, IMemoryCache cache)
        {
            _env = env;
            _cache = cache;
            _htmlFolderPath = Path.Combine(_env.WebRootPath, "html");
        }

        public List<Article> GetRecentArticles(int count = 5)
        {
            string cacheKey = $"CACHE_{count}";

            // Controlla se gli articoli sono già in cache
            if (_cache.TryGetValue(cacheKey, out List<Article>? cachedArticles))
            {
                return cachedArticles!;
            }

            if (!Directory.Exists(_htmlFolderPath))
                return [];

            var articles = Directory.GetFiles(_htmlFolderPath, "*.html", SearchOption.AllDirectories)
                .Select(ReadArticleFromFile)
                .Where(a => a != null)
                .OrderByDescending(a => a!.PublicationDate)
                .Take(count)
                .ToList();

            // Salva il risultato nella cache per 20 minuti
            _cache.Set(cacheKey, articles, TimeSpan.FromMinutes(20));

            return articles!;
        }

        private Article? ReadArticleFromFile(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);

                // Estrai il titolo <h1>
                string title = Regex.Match(content, @"<h1[^>]*>(.*?)<\/h1>", RegexOptions.IgnoreCase)
                                    .Groups[1].Value.Trim();

                // Estrai la data da <datapub>
                string dateStr = Regex.Match(content, @"<datapub>(.*?)<\/datapub>", RegexOptions.IgnoreCase)
                                      .Groups[1].Value.Trim();

                if (DateTime.TryParseExact(dateStr, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out DateTime publicationDate))
                {
                    return new Article
                    {
                        Title = title.RemoveHtmlTags(),
                        PublicationDate = publicationDate,
                        FilePath = GetArticleUrl(filePath)
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore lettura file {filePath}: {ex.Message}");
            }

            return null;
        }

        public string GetArticleUrl(string filePath)
        {
            string relativePath = filePath.Replace(_htmlFolderPath, "").Replace("\\", "/");
            return $"/HtmlViewer{relativePath}";
        }
    }

    public class Article
    {
        public string Title { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
}
