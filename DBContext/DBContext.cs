using Microsoft.EntityFrameworkCore;
using Website_parser.Models;

namespace Website_parser.DbContexts
{
    public class WebSiteDBContext : DbContext
    {
        public DbSet<DBRecord> Articles { get; set; }
        public DbSet<Site> Sites { get; set; }

        public WebSiteDBContext(DbContextOptions<WebSiteDBContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}

