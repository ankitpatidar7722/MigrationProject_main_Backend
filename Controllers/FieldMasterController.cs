using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FieldMasterController : ControllerBase
{
    private readonly MigraTrackDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    public FieldMasterController(MigraTrackDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private void InvalidateCache()
    {
        _cache.Remove("fieldmaster_all");
        // Also invalidate group-specific caches — simpler to just remove a known prefix pattern
        // Since IMemoryCache doesn't support prefix removal, we clear the "all" cache
        // and let group caches expire naturally (60s TTL)
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FieldMaster>>> GetAll()
    {
        var data = await _cache.GetOrCreateAsync("fieldmaster_all", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _context.FieldMaster
                .AsNoTracking()
                .Where(f => f.IsActive)
                .ToListAsync();
        });
        return Ok(data);
    }

    [HttpGet("group/{moduleGroupId}")]
    public async Task<ActionResult<IEnumerable<FieldMaster>>> GetByModuleGroup(int moduleGroupId)
    {
        var cacheKey = $"fieldmaster_group_{moduleGroupId}";
        var data = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _context.FieldMaster
                .AsNoTracking()
                .Where(f => f.ModuleGroupId == moduleGroupId && f.IsActive)
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();
        });
        return Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FieldMaster>> GetById(int id)
    {
        var field = await _context.FieldMaster.FindAsync(id);
        if (field == null)
            return NotFound();

        return Ok(field);
    }

    [HttpPost]
    public async Task<ActionResult<FieldMaster>> Create(FieldMaster field)
    {
        field.CreatedAt = DateTime.Now;
        field.UpdatedAt = DateTime.Now;

        _context.FieldMaster.Add(field);
        await _context.SaveChangesAsync();
        InvalidateCache();

        return CreatedAtAction(nameof(GetById), new { id = field.FieldId }, field);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, FieldMaster field)
    {
        if (id != field.FieldId)
            return BadRequest();

        var existing = await _context.FieldMaster.FindAsync(id);
        if (existing == null)
            return NotFound();

        existing.FieldName = field.FieldName;
        existing.FieldLabel = field.FieldLabel;
        existing.FieldDescription = field.FieldDescription;
        existing.DataType = field.DataType;
        existing.IsRequired = field.IsRequired;
        existing.DefaultValue = field.DefaultValue;
        existing.SelectQueryDb = field.SelectQueryDb;
        existing.DisplayOrder = field.DisplayOrder;

        existing.ValidationRegex = field.ValidationRegex;
        existing.PlaceholderText = field.PlaceholderText;
        existing.HelpText = field.HelpText;
        existing.IsUnique = field.IsUnique;
        existing.MaxLength = field.MaxLength;
        existing.IsDisplay = field.IsDisplay;

        existing.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        InvalidateCache();
        return Ok(existing);
    }

    [HttpGet("lookup/{type}")]
    public async Task<ActionResult<IEnumerable<object>>> GetLookupValues(string type)
    {
        var cacheKey = $"lookup_{type}";
        var data = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _context.LookupData
                .AsNoTracking()
                .Where(l => l.LookupType == type && l.IsActive)
                .OrderBy(l => l.DisplayOrder)
                .Select(l => new { l.LookupKey, l.LookupValue })
                .ToListAsync();
        });
        return Ok(data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var field = await _context.FieldMaster.FindAsync(id);
        if (field == null)
            return NotFound();

        field.IsActive = false;
        await _context.SaveChangesAsync();
        InvalidateCache();

        return NoContent();
    }
}
