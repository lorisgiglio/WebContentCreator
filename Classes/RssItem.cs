namespace WebContentCreator.Classes;

public class RssItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string PubDate { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public DateTime? PubDateTime
    {
        get
        {
            if (DateTime.TryParse(PubDate, out DateTime parsedDate))
            {
                return parsedDate;
            }
            return null;
        }
    }
}
