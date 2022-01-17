
namespace Website_parser.Models
{
    public class Article
    {
        public int Id { get; set; }
        public string HtmlText { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Text { get; set; }
        public string Date { get; set; }
    }
}