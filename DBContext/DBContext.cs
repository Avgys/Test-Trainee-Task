using Microsoft.EntityFrameworkCore;
using Website_parser.Models;

namespace Website_parser.DbContexts
{
    public class WebSiteDBContext : DbContext
    {
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleEntityPair> InfoPairs { get; set; }
        public DbSet<Entity> Entyties { get; set; }
        public DbSet<Site> Sites { get; set; }

        public WebSiteDBContext(DbContextOptions<WebSiteDBContext> options) : base(options)
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }
    }
}

