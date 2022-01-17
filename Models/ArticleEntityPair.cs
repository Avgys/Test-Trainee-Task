using Pullenti.Ner;

namespace Website_parser.Models
{
    public class ArticleEntityPair
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public int EntityId { get; set; }
    }

    public class ArticleEntity
    {
        public Article Article { get; set; }
        public Entity Entity { get; set; }
    }
}