using System.Net;
using System.Text.RegularExpressions;

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
    }

}
