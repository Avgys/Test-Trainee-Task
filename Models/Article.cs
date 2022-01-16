
namespace Website_parser.Models
{
    public class Article
    {
        public int id { get; set; }
        public string htmlText { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string text { get; set; }
        public string date { get; set; }
    }
}