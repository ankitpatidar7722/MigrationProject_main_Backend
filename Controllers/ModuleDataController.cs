using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModuleDataController : ControllerBase
{
    private readonly MigraTrackDbContext _context;

    public ModuleDataController(MigraTrackDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DynamicModuleData>>> GetModuleData(
        [FromQuery] long projectId,
        [FromQuery] int moduleGroupId)
    {
        var data = await _context.DynamicModuleData
            .Where(d => d.ProjectId == projectId && 
                       d.ModuleGroupId == moduleGroupId && 
                       !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DynamicModuleData>> GetById(string id)
    {
        var data = await _context.DynamicModuleData.FindAsync(id);
        if (data == null || data.IsDeleted)
            return NotFound();

        return Ok(data);
    }

    [HttpPost]
    public async Task<ActionResult<DynamicModuleData>> Create(DynamicModuleData data)
    {
        data.CreatedAt = DateTime.Now;
        data.UpdatedAt = DateTime.Now;
        
        _context.DynamicModuleData.Add(data);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = data.RecordId }, data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, DynamicModuleData data)
    {
        if (id != data.RecordId)
            return BadRequest();

        var existing = await _context.DynamicModuleData.FindAsync(id);
        if (existing == null || existing.IsDeleted)
            return NotFound();

        existing.JsonData = data.JsonData;
        existing.Status = data.Status;
        existing.IsCompleted = data.IsCompleted;
        existing.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var data = await _context.DynamicModuleData.FindAsync(id);
        if (data == null)
            return NotFound();

        data.IsDeleted = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
