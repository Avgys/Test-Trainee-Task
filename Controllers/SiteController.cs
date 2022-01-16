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
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;

namespace Website_parser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteController : ControllerBase
    {
        private readonly WebSiteDBContext _context;
        private static object ArticlesSync = new object();

        public SiteController(WebSiteDBContext context)
        {
            _context = context;
        }

        // GET: api/Sites
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Site>>> GetArticles([FromQuery] string keyword)
        {
            if (!String.IsNullOrEmpty(keyword))
            {
                var articles = _context.Articles.Take(10).ToList();
                var sites = articles
                    .Where(a => a.text.Contains(keyword))
                    .Select(a => new Site() { url = a.url })
                    .ToList();
                return sites;
            }
            //return await _context.Sites.ToListAsync();
            return NotFound();
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
            bool isUrlValid = Uri.TryCreate(site.url, UriKind.Absolute, out uri);
            if (isUrlValid)
            {
                var articles = await CrawlSite(uri);
                bool isDuplicate = await _context.Sites.AnyAsync(e => e.url == site.url);
                if (!isDuplicate)
                {
                    _context.Sites.Add(site);
                    await _context.SaveChangesAsync();
                }

                return CreatedAtAction("GetSite", new { id = site.id }, site);
            }
            else
            {
                return BadRequest("Wrong url param");
            }
        }

        private async Task<IEnumerable<Article>> CrawlSite(Uri uri)
        {
            var config = new CrawlConfiguration
            {
                MaxPagesToCrawl = 10,
                MinCrawlDelayPerDomainMilliSeconds = 50 //Wait this many millisecs between requests
            };

            var crawler = new PoliteWebCrawler(config);
            var articles = new List<Article>();
            crawler.CrawlBag.Articles = articles;
            crawler.PageCrawlCompleted += PageCrawlCompleted;//Several events available...

            var crawlResult = await crawler.CrawlAsync(uri);
            foreach (var a in articles) {
                bool isDuplicate = await _context.Articles.AnyAsync(e => e.url == a.url);
                if (!isDuplicate)
                {
                    _context.Articles.Add(a);
                }
            }

            await _context.SaveChangesAsync();
            return articles;
        }

        private static async void PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            if (e.CrawledPage.HttpRequestException != null || e.CrawledPage.HttpResponseMessage.StatusCode != HttpStatusCode.OK)
                Console.WriteLine($"Crawl of page failed {e.CrawledPage.Uri.AbsoluteUri}");
            else
                Console.WriteLine($"Crawl of page succeeded {e.CrawledPage.Uri.AbsoluteUri}");

            if (string.IsNullOrEmpty(e.CrawledPage.Content.Text))
                Console.WriteLine($"Page had no content {e.CrawledPage.Uri.AbsoluteUri}");

            Article article = new Article();
            article.title = e.CrawledPage.AngleSharpHtmlDocument.Title;
            var body = e.CrawledPage.AngleSharpHtmlDocument.Body.OuterHtml;
            var metas = e.CrawledPage.AngleSharpHtmlDocument
                .GetElementsByTagName("meta")
                .Where(e => e.GetAttribute("property") == "article:published_time")
                .ToList();
            if (metas.Count > 0)
            {
                article.date = metas[0].GetAttribute("content");
            }
            article.url = e.CrawledPage.Uri.AbsoluteUri;
            article.htmlText = e.CrawledPage.Content.Text;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(body);
            String plainBody = document.DocumentNode.InnerText;
            plainBody = Regex.Replace(plainBody, "[\n\t]", String.Empty);
            article.text = plainBody.Trim().Replace("  "," ");
            lock (e.CrawlContext.CrawlBag.Articles)
                e.CrawlContext.CrawlBag.Articles.Add(article);
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
