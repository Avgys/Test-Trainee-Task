using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_parser.DbContexts;
using Website_parser.Models;

namespace Website_parser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DBRecordsController : ControllerBase
    {
        private readonly WebSiteDBContext _context;

        public DBRecordsController(WebSiteDBContext context)
        {
            _context = context;
        }

        // GET: api/DBRecords
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DBRecord>>> GetArticles()
        {
            return await _context.Articles.ToListAsync();
        }

        // GET: api/DBRecords/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DBRecord>> GetDBRecord(int id)
        {
            var dBRecord = await _context.Articles.FindAsync(id);

            if (dBRecord == null)
            {
                return NotFound();
            }

            return dBRecord;
        }

        // PUT: api/DBRecords/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDBRecord(int id, DBRecord dBRecord)
        {
            if (id != dBRecord.id)
            {
                return BadRequest();
            }

            _context.Entry(dBRecord).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DBRecordExists(id))
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

        // POST: api/DBRecords
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Site>> PostDBRecord(Site site)
        {
            _context.Sites.Add(site);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDBRecord", new { id = site.id }, site);
        }

        // DELETE: api/DBRecords/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDBRecord(int id)
        {
            var dBRecord = await _context.Articles.FindAsync(id);
            if (dBRecord == null)
            {
                return NotFound();
            }

            _context.Articles.Remove(dBRecord);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DBRecordExists(int id)
        {
            return _context.Articles.Any(e => e.id == id);
        }
    }
}
