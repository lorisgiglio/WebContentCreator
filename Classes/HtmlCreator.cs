using Serilog;
using System.Globalization;
using System.Text.RegularExpressions;
using static WebContentCreator.Classes.HtmlHelper;

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
            string dataOggi = DateTime.Now.ToString("yyyyMMdd");
            string dataPubblicazione = DateTime.Now.ToString();

            if (DateTime.TryParse(argomento.PubDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime data))
            {
                dataPubblicazione = data.ToString("dd-MM-yyyy HH:mm");
            }

            string arg = SanitizeFileName(argomento.Title);
            string topic = SanitizeFileName(argomento.Topic).ToCamelCase();
            string nomeFile = $"{lingua}_{topic}_{arg}_{customhash}.html";

            contenuto = CleanHtml(contenuto).RemoveNewLines();

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
                                        "Evita testi dove non puoi citare le persone coinvolte (quindi senza riferimenti tra parentesi tonde o quadre) e il testo deve essere originale in modo da non incorrere in violazioni di copyright." +
                                        "L'evento si riferisce a quest'anno, quindi cerca tra le notizie recenti e non di mesi o anni fa." +
                                        "Sostituisci i ritorni a capo con i tag BR, non includere tag html-head-body. Rimuovi i link con example.com e example.it e i commenti tra quadre e tonde. " +
                                        "Non inserire link a testate giornalistiche come Ansa, AffariItaliani e non inserire commenti come : [inserire qui un link a una galleria fotografica o a un approfondimento online]";

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
    }

}
