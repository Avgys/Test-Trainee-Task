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
using Pullenti;
using Pullenti.Ner;

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
            Pullenti.Sdk.InitializeAll();
        }

        // GET: api/Sites
        [HttpGet]
        public async Task<ActionResult<IEnumerable<String>>> GetArticles([FromQuery] string word, [FromQuery] string entityName, [FromQuery] string entityAttribute)
        {
            if (!String.IsNullOrEmpty(word))
            {
                var sites = _context.Articles
                    .Where(a => a.Text.Contains(word))
                    .Select(a => a.Url)
                    .ToList();

                Console.WriteLine("Find {0} word", word);

                foreach(var site in sites)
                {
                    Console.WriteLine(site);
                }

                Console.WriteLine("Found {0} matches", sites.Count);
                return sites;
            }

            if (!String.IsNullOrEmpty(entityName))
            {
                var sites = (from ent in _context.Entities
                            join pair in _context.InfoPairs on ent.Id equals pair.EntityId
                            join article in _context.Articles on pair.ArticleId equals article.Id
                            where ent.Name == entityName 
                            select article.Url).ToList();

                Console.WriteLine("Find {0} word", word);
                foreach (var site in sites)
                {
                    Console.WriteLine(site);
                }

                Console.WriteLine("Found {0} matches", sites.Count);

                return sites;
            }

            if (!String.IsNullOrEmpty(entityAttribute))
            {                
                var sites = (from attr in _context.EntityAttributes
                                join ent in _context.Entities on attr.ParentEntityId equals ent.Id
                                join pair in _context.InfoPairs on ent.Id equals pair.EntityId
                                join article in _context.Articles on pair.ArticleId equals article.Id
                                where attr.Value == entityAttribute
                                select article.Url).ToList();

                Console.WriteLine("Find {0} word", word);
                foreach (var site in sites)
                {
                    Console.WriteLine(site);
                }

                Console.WriteLine("Found {0} matches", sites.Count);
                return sites;
            }
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
            if (id != Site.Id)
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
            bool isUrlValid = Uri.TryCreate(site.Url, UriKind.Absolute, out uri);
            if (isUrlValid)
            {
                var articles = await CrawlSite(uri);
                bool isDuplicate = await _context.Sites.AnyAsync(e => e.Url == site.Url);
                if (!isDuplicate)
                {
                    _context.Sites.Add(site);
                    await _context.SaveChangesAsync();
                }

                return CreatedAtAction("GetSite", new { id = site.Id }, site);
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
            for (int i = articles.Count - 1; i >= 0 ; i--)
            {
                bool isDuplicate = await _context.Articles.AnyAsync(e => e.Url == articles[i].Url);
                if (!isDuplicate)
                {
                    _context.Articles.Add(articles[i]);
                }
                else
                {
                    articles.Remove(articles[i]);
                }
            }

            await _context.SaveChangesAsync();

            Console.WriteLine("CrawlPage is completed.");
            
            Processor processor = ProcessorService.CreateProcessor();
            List<ArticleEntity> listPairs = new();
            Dictionary<string, Entity> entList = new();
            List<Models.Attribute> attrList = new();

            Console.WriteLine("Finding entities in text started.");
            foreach (var a in articles)
            {
                if (a.Id != 0)
                {
                    AnalysisResult result = processor.Process(new SourceOfAnalysis(a.Text));
                    foreach (Referent entity in result.Entities)
                    {
                        string key =  entity.ToString();
                        Entity entOut;
                        if (!entList.TryGetValue(key, out entOut))
                        {
                            var ent = new Entity() { Name = key };
                            entList.Add(key, ent);

                            foreach (var slot in entity.Slots)
                            {                                
                                if (slot.Value is string)
                                {
                                    attrList.Add(new Models.Attribute()
                                    {
                                        ParentEntity = ent,
                                        Value = slot.Value as string,
                                        Type = slot.TypeName
                                    });
                                }
                            }
                            
                            _context.Entities.Add(ent);
                            listPairs.Add(new ArticleEntity() { Article = a, Entity = ent });
                        }
                        else
                        {
                            listPairs.Add(new ArticleEntity() { Article = a, Entity = entOut });
                            
                            Console.WriteLine("Common entities found {0}.",entOut.Name);
                        }
                        
                    }
                }
            }


            Console.WriteLine("Finding entities in text ended.");

            await _context.SaveChangesAsync();

            foreach(var attr in attrList)
            {
                attr.ParentEntityId = attr.ParentEntity.Id;
                _context.EntityAttributes.Add(attr);
            }

            await _context.SaveChangesAsync();

            Console.WriteLine("Adding pairs of entities and articles started.");
            foreach (var pair in listPairs)
            {
                _context.InfoPairs.Add(new ArticleEntityPair() { ArticleId = pair.Article.Id, EntityId = pair.Entity.Id});
            }

            await _context.SaveChangesAsync();

            Console.WriteLine("Adding pairs of entities and articles ended.");

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
            article.Title = e.CrawledPage.AngleSharpHtmlDocument.Title;
            var body = e.CrawledPage.AngleSharpHtmlDocument.Body.OuterHtml;
            var metas = e.CrawledPage.AngleSharpHtmlDocument
                .GetElementsByTagName("meta")
                .Where(e => e.GetAttribute("property") == "article:published_time")
                .ToList();
            
            if (metas.Count > 0)
            {
                article.Date = metas[0].GetAttribute("content");
            }

            article.Url = e.CrawledPage.Uri.AbsoluteUri;
            article.HtmlText = e.CrawledPage.Content.Text;
            
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(body);
            String plainBody = document.DocumentNode.InnerText;
            plainBody = Regex.Replace(plainBody, "[\n\t]", String.Empty);
            article.Text = plainBody.Trim().Replace("  ", " ");
            
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
            return _context.Articles.Any(e => e.Id == id);
        }
    }
}
