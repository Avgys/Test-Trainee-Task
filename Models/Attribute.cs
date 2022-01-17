using System.ComponentModel.DataAnnotations.Schema;

namespace Website_parser.Models
{
    public class Attribute
    {
        public int Id { get; set; }
        [NotMapped]
        public Entity ParentEntity { get; set; }
        public int ParentEntityId { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
