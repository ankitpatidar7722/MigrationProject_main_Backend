using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ModuleMasterController : ControllerBase
{
    private readonly MigraTrackDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    public ModuleMasterController(MigraTrackDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private void InvalidateCache() => _cache.Remove("modulemaster_all");

    // GET: api/ModuleMaster
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ModuleMaster>>> GetModuleMasters()
    {
        var data = await _cache.GetOrCreateAsync("modulemaster_all", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _context.ModuleMasters
                .AsNoTracking()
                .OrderBy(m => m.ModuleName)
                .ThenBy(m => m.SubModuleName)
                .ToListAsync();
        });
        return Ok(data);
    }

    // GET: api/ModuleMaster/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ModuleMaster>> GetModuleMaster(int id)
    {
        var moduleMaster = await _context.ModuleMasters.FindAsync(id);

        if (moduleMaster == null)
        {
            return NotFound();
        }

        return moduleMaster;
    }

    // PUT: api/ModuleMaster/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutModuleMaster(int id, ModuleMaster moduleMaster)
    {
        if (id != moduleMaster.ModuleId)
        {
            return BadRequest();
        }

        _context.Entry(moduleMaster).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            InvalidateCache();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ModuleMasterExists(id))
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

    // POST: api/ModuleMaster
    [HttpPost]
    public async Task<ActionResult<ModuleMaster>> PostModuleMaster(ModuleMaster moduleMaster)
    {
        _context.ModuleMasters.Add(moduleMaster);
        await _context.SaveChangesAsync();
        InvalidateCache();

        return CreatedAtAction("GetModuleMaster", new { id = moduleMaster.ModuleId }, moduleMaster);
    }

    // DELETE: api/ModuleMaster/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteModuleMaster(int id)
    {
        var moduleMaster = await _context.ModuleMasters.FindAsync(id);
        if (moduleMaster == null)
        {
            return NotFound();
        }

        _context.ModuleMasters.Remove(moduleMaster);
        await _context.SaveChangesAsync();
        InvalidateCache();

        return NoContent();
    }

    private bool ModuleMasterExists(int id)
    {
        return _context.ModuleMasters.Any(e => e.ModuleId == id);
    }
}
