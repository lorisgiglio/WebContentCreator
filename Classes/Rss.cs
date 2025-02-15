using System.Collections.Concurrent;
using System.Xml.Linq;

namespace WebContentCreator.Classes
{
    public class Rss
    {
        static async Task<List<RssItem>> FetchRssFeed(string url)
        {
            using HttpClient client = new HttpClient();
            string xmlContent = await client.GetStringAsync(url);
            XDocument doc = XDocument.Parse(xmlContent);

            var items = new ConcurrentBag<RssItem>();
            Parallel.ForEach(doc.Descendants("item"), item =>
            {
                items.Add(new RssItem
                {
                    Title = item.Element("title")?.Value.Trim()!,
                    Description = item.Element("description")?.Value.Trim()!,
                    Link = item.Element("link")?.Value.Trim()!,
                    PubDate = item.Element("pubDate")?.Value.Trim()!
                });
            });

            return items.ToList();
        }

        public static List<RssItem> GenerateRssItems()
        {
            List<KeyValuePair<string, string>> urls = new List<KeyValuePair<string, string>>
            {
            new KeyValuePair<string, string>("Home", "https://www.ansa.it/sito/ansait_rss.xml"),
            new KeyValuePair<string, string>("Cronaca", "https://www.ansa.it/sito/notizie/cronaca/cronaca_rss.xml"),
            new KeyValuePair<string, string>("Politica", "https://www.ansa.it/sito/notizie/politica/politica_rss.xml"),
            new KeyValuePair<string, string>("Mondo", "https://www.ansa.it/sito/notizie/mondo/mondo_rss.xml"),
            new KeyValuePair<string, string>("Economia", "https://www.ansa.it/sito/notizie/economia/economia_rss.xml"),
            new KeyValuePair<string, string>("Calcio", "https://www.ansa.it/sito/notizie/sport/calcio/calcio_rss.xml"),
            new KeyValuePair<string, string>("Sport", "https://www.ansa.it/sito/notizie/sport/sport_rss.xml"),
            new KeyValuePair<string, string>("Cinema", "https://www.ansa.it/sito/notizie/cultura/cinema/cinema_rss.xml"),
            new KeyValuePair<string, string>("Cultura", "https://www.ansa.it/sito/notizie/cultura/cultura_rss.xml"),
            new KeyValuePair<string, string>("Ultime Notizie", "https://www.ansa.it/sito/notizie/topnews/topnews_rss.xml"),
            new KeyValuePair<string, string>("Foto", "https://www.ansa.it/sito/photogallery/foto_rss.xml"),
            new KeyValuePair<string, string>("Video", "https://www.ansa.it/sito/videogallery/video_rss.xml")
        };

            var items = new ConcurrentBag<RssItem>();
            Parallel.ForEach(urls, url =>
            {
                List<RssItem> rssItems = FetchRssFeed(url.Value).Result;
                foreach (var item in rssItems)
                {
                    item.Topic = url.Key;
                    items.Add(item);
                    //Console.WriteLine($"{url.Key}-Title: {item.Title} ({item.PubDate})");
                    //Console.Write($"-Description: {item.Description}");
                    //Console.Write($"-Link: {item.Link}");
                    //Console.Write($"-Published Date: {item.PubDate}");
                    //Console.WriteLine(new string('-', 50));
                }
            });
            return items.ToList();
        }
    }
}
