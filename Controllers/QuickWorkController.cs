using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MigraTrackAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuickWorksController : ControllerBase
    {
        private readonly MigraTrackDbContext _context;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

        public QuickWorksController(MigraTrackDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        private void InvalidateCache() => _cache.Remove("quickworks_all");

        // GET: api/QuickWorks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuickWork>>> GetQuickWorks()
        {
            var data = await _cache.GetOrCreateAsync("quickworks_all", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                return await _context.QuickWorks.AsNoTracking().ToListAsync();
            });
            return Ok(data);
        }

        // GET: api/QuickWorks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuickWork>> GetQuickWork(int id)
        {
            var quickWork = await _context.QuickWorks.FindAsync(id);

            if (quickWork == null)
            {
                return NotFound();
            }

            return quickWork;
        }

        // PUT: api/QuickWorks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuickWork(int id, QuickWork quickWork)
        {
            if (id != quickWork.Id)
            {
                return BadRequest();
            }

            quickWork.UpdatedAt = DateTime.Now;
            _context.Entry(quickWork).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                InvalidateCache();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuickWorkExists(id))
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

        // POST: api/QuickWorks
        [HttpPost]
        public async Task<ActionResult<QuickWork>> PostQuickWork(QuickWork quickWork)
        {
            quickWork.CreatedAt = DateTime.Now;
            quickWork.UpdatedAt = DateTime.Now;
            _context.QuickWorks.Add(quickWork);
            await _context.SaveChangesAsync();
            InvalidateCache();

            return CreatedAtAction("GetQuickWork", new { id = quickWork.Id }, quickWork);
        }

        // DELETE: api/QuickWorks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuickWork(int id)
        {
            var quickWork = await _context.QuickWorks.FindAsync(id);
            if (quickWork == null)
            {
                return NotFound();
            }

            _context.QuickWorks.Remove(quickWork);
            await _context.SaveChangesAsync();
            InvalidateCache();

            return NoContent();
        }

        private bool QuickWorkExists(int id)
        {
            return _context.QuickWorks.Any(e => e.Id == id);
        }
    }
}
