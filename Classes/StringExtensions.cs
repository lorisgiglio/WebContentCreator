using System.Globalization;
using System.Text.RegularExpressions;

namespace WebContentCreator.Classes
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            str = Regex.Replace(str, @"[^a-zA-Z0-9\s]", " ");
            str = Regex.Replace(str, @"\s+", " ").Trim();

            if (string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }

            // Converte in Title Case (prima lettera di ogni parola maiuscola)
            TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
            str = textInfo.ToTitleCase(str);

            // Rimuovi spazi e metti la prima lettera minuscola
            str = str.Replace(" ", "");
            return str.Substring(0, 1).ToLower() + str.Substring(1);
        }
        public static string ToCamelCaseInvariant(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            // Rimuovi caratteri non alfanumerici e spazi, sostituiscili con un singolo spazio
            str = Regex.Replace(str, @"[^a-zA-Z0-9\s]", " ");
            str = Regex.Replace(str, @"\s+", " ").Trim();

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty; // Gestisci il caso di stringa vuota dopo la pulizia
            }

            // Converte in Title Case (prima lettera di ogni parola maiuscola) usando CultureInfo.InvariantCulture
            TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
            str = textInfo.ToTitleCase(str);

            // Rimuovi spazi e metti la prima lettera minuscola
            str = str.Replace(" ", "");
            return str.Substring(0, 1).ToLower() + str.Substring(1);
        }
        public static string ExtractFirstH1(this string filePath)
        {
            if (!File.Exists(filePath))
                return string.Empty;

            string content = File.ReadAllText(filePath);
            Match match = Regex.Match(content, "<h1.*?>(.*?)</h1>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }
        public static string ExtractH1(this string filePath, int occurrence = 1)
        {
            if (!File.Exists(filePath))
                return string.Empty;

            string content = File.ReadAllText(filePath);
            MatchCollection matches = Regex.Matches(content, "<h1.*?>(.*?)</h1>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (matches.Count >= occurrence && occurrence > 0)
            {
                return matches[occurrence - 1].Groups[1].Value.Trim();
            }

            return string.Empty;
        }
        public static string ExtractFirstDataPub(this string filePath)
        {
            if (!File.Exists(filePath))
                return string.Empty;

            string content = File.ReadAllText(filePath);
            Match match = Regex.Match(content, "<datapub.*?>(.*?)</datapub>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            return match.Success ? match.Groups[1].Value.Trim() : DateTime.Now.ToString();
        }
        // Lista di parole da escludere, come articoli, preposizioni, congiunzioni, ecc.
        private static HashSet<string> stopWords = new HashSet<string>(new string[]
        {
        "il", "la", "i", "gli", "le", "un", "una", "lo", "gli", "di", "a", "da", "in", "su", "per", "con", "tra", "fra",
        "e", "ma", "se", "che", "quando", "dove", "come", "non", "di", "ne", "al", "alla", "alle", "dal", "dallo", "dalla",
        "nel", "sul", "lo", "là", "più", "meno", "cosa", "questa", "questo", "questa", "queste", "è", "sono", "tu", "io", "sì",
        "del", "sua", "ha", "nelle", "dalle", "della", "dello", "delle", "dei", "degli", "dell", "dell'", "dai", "dagli", "dalle",
        "dalla", "dallo", "dell", "dell'", "dei", "degli", "dell", "dell'", "dai", "dagli", "dalle", "dalla", "dallo", "dell", "dell'",
        "si", "ogni", "tutti", "tutto", "tutta"
        });

        // Funzione per estrarre le keyword
        public static string ExtractKeywords(this string filePath)
        {
            if (!File.Exists(filePath))
                return string.Empty;

            string text = File.ReadAllText(filePath);

            // Puliamo il testo rimuovendo la punteggiatura e trasformando in minuscolo
            var cleanedText = Regex.Replace(text.ToLower(), @"[^\w\s]", "");

            // Suddividiamo il testo in parole
            var words = cleanedText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Filtriamo le parole rimuovendo le stop words
            var filteredWords = words.Where(word => !stopWords.Contains(word)).ToList();

            // Ordiniamo le parole per frequenza
            var wordFrequency = filteredWords
                .GroupBy(word => word)
                .OrderByDescending(group => group.Count())
                .Take(6)  // Limitiamo a 6 parole più significative
                .Select(group => group.Key)
                .ToList();

            return string.Join(" ", wordFrequency);
        }
    }
}
