using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_parser.DbContexts;
using Website_parser.Models;
using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;

namespace Website_parser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteController : ControllerBase
    {
        private readonly WebSiteDBContext _context;

        public SiteController(WebSiteDBContext context)
        {
            _context = context;
        }

        // GET: api/Sites
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Site>>> GetArticles()
        {
            return await _context.Sites.ToListAsync();
        }

        // GET: api/Sites/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Site>> GetSite(int id)
        {
            var site = await _context.Sites.FindAsync(id);

            if (site == null)
            {
                return NotFound();
            }

            return site;
        }

        // PUT: api/Sites/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSite(int id, Site Site)
        {
            if (id != Site.id)
            {
                return BadRequest();
            }

            _context.Entry(Site).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SiteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Sites
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Site>> PostSite(Site site)
        {
            Uri uri;
            bool isUrlValid = Uri.TryCreate("http://" + site.url, UriKind.RelativeOrAbsolute, out uri);
            if (isUrlValid) {
                await CrawlSite(uri);
                _context.Sites.Add(site);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetSite", new { id = site.id }, site);
            }
            else
            {
                return BadRequest("Wrong url param");
            }
        }

        private async Task CrawlSite(Uri uri)
        {
            var config = new CrawlConfiguration
            {
                MaxPagesToCrawl = 10,
                MinCrawlDelayPerDomainMilliSeconds = 50 //Wait this many millisecs between requests
            };
            
            var crawler = new PoliteWebCrawler(config);

            crawler.PageCrawlCompleted += PageCrawlCompleted;//Several events available...
            
            var crawlResult = await crawler.CrawlAsync(uri);
        }

        private static void PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            var httpStatus = e.CrawledPage.HttpResponseMessage.StatusCode;
            //var htmlText = e.CrawledPage.Content.;
            var rawPageText = e.CrawledPage.Content.Text;
        }

        // DELETE: api/Sites/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSite(int id)
        {
            var Site = await _context.Articles.FindAsync(id);
            if (Site == null)
            {
                return NotFound();
            }

            _context.Articles.Remove(Site);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SiteExists(int id)
        {
            return _context.Articles.Any(e => e.id == id);
        }
    }
}
