
namespace Website_parser.Models
{
    public class Article
    {
        public int id { get; set; }
        public string htmlText { get; set; }
        public string articleName { get; set; }
        public string url { get; set; }
        public string articleText { get; set; }
        public string articleDate { get; set; }
    }
}