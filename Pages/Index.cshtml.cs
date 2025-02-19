using Microsoft.AspNetCore.Mvc.RazorPages;
using WebContentCreator.Classes;

namespace WebContentCreator
{
    public class IndexModel : PageModel
    {
        private readonly HtmlArticleService _articleService;
        public List<Article> RecentArticles { get; set; } = new();

        private readonly IWebHostEnvironment _env;
        public Dictionary<string, List<NewsItem>> NewsByTopic { get; set; } = new();
        public List<string> Topics { get; set; } = [];
        public IndexModel(IWebHostEnvironment env, HtmlArticleService articleService)
        {
            _env = env;
            _articleService = articleService;
        }
        public void OnGet()
        {
            string basePath = Path.Combine(_env.WebRootPath, "html");

            if (!Directory.Exists(basePath))
                return;

            var allFiles = Directory.GetDirectories(basePath)
                .SelectMany(dir => Directory.GetFiles(dir, "*.html"))
                .ToList();

            foreach (var file in allFiles)
            {
                string fileName = Path.GetFileName(file);
                string directoryName = new DirectoryInfo(Path.GetDirectoryName(file)!).Name;

                var parts = fileName.Split('_');
                if (parts.Length < 3) continue;

                string topic = parts[1];

                if (!NewsByTopic.ContainsKey(topic))
                    NewsByTopic[topic] = new List<NewsItem>();

                string title = string.Empty;

                NewsByTopic[topic].Add(new NewsItem
                {
                    Title = file.ExtractFirstH1(),
                    FilePath = $"{directoryName}/{fileName}",
                    Date = DateTime.Parse(file.ExtractFirstDataPub())
                });
            }

            // Ordina le notizie per data

            foreach (var topic in NewsByTopic.Keys)
            {
                NewsByTopic[topic] = NewsByTopic[topic].OrderByDescending(n => n.Date).ToList();
            }

            Topics = NewsByTopic.Keys.OrderBy(t => t).ToList();
            RecentArticles = _articleService.GetRecentArticles(5);
        }
        public class NewsItem
        {
            public string Title { get; set; } = string.Empty;
            public string FilePath { get; set; } = string.Empty;
            public DateTime Date { get; set; } = DateTime.MinValue;
        }
    }
}