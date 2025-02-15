using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebContentCreator.Classes;

namespace WebContentCreator
{
    public class HtmlViewerModel : PageModel
    {
        private readonly IWebHostEnvironment _env;
        public string HtmlContent { get; private set; } = string.Empty;
        public HtmlViewerModel(IWebHostEnvironment env)
        {
            _env = env;
        }
        public void OnGet(string fileName)
        {
            string filePath = Path.Combine(_env.WebRootPath, "html", fileName);

            if (System.IO.File.Exists(filePath))
            {
                HtmlContent = System.IO.File.ReadAllText(filePath).RemoveExampleLinks();
                ViewData["Title"] = filePath.ExtractH1(1).RemoveHtmlTags().Replace("\"", "'");
                ViewData["Description"] = filePath.ExtractH1(2).RemoveHtmlTags().Replace("\"", "'");
                ViewData["CurrentUrl"] = Request.GetEncodedUrl();
                ViewData["Keywords"] = filePath.ExtractKeywords();
                ViewData["DataPubblicazione"] = filePath.ExtractFirstDataPub();
            }
            else
            {
                HtmlContent = "<p>Il file non esiste.</p>";
            }
        }
    }
}