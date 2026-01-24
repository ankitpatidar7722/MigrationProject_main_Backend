using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ManualConfigurationsController : ControllerBase
{
    private readonly MigraTrackDbContext _context;

    public ManualConfigurationsController(MigraTrackDbContext context)
    {
        _context = context;
    }

    // GET: api/ManualConfigurations/project/5
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<ManualConfiguration>>> GetByProject(long projectId)
    {
        return await _context.ManualConfigurations
            .Where(m => m.ProjectId == projectId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    // GET: api/ManualConfigurations/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ManualConfiguration>> GetManualConfiguration(int id)
    {
        var manualConfiguration = await _context.ManualConfigurations.FindAsync(id);

        if (manualConfiguration == null)
        {
            return NotFound();
        }

        return manualConfiguration;
    }

    // POST: api/ManualConfigurations
    [HttpPost]
    public async Task<ActionResult<ManualConfiguration>> PostManualConfiguration(ManualConfiguration manualConfiguration)
    {
        // Ensure Project exists
        var project = await _context.Projects.FindAsync(manualConfiguration.ProjectId);
        if (project == null)
        {
            return BadRequest("Invalid Project ID");
        }

        manualConfiguration.CreatedAt = DateTime.UtcNow;
        manualConfiguration.UpdatedAt = null;

        _context.ManualConfigurations.Add(manualConfiguration);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetManualConfiguration", new { id = manualConfiguration.Id }, manualConfiguration);
    }

    // PUT: api/ManualConfigurations/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutManualConfiguration(int id, ManualConfiguration manualConfiguration)
    {
        if (id != manualConfiguration.Id)
        {
            return BadRequest();
        }

        var existing = await _context.ManualConfigurations.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.ModuleName = manualConfiguration.ModuleName;
        existing.SubModuleName = manualConfiguration.SubModuleName;
        existing.Description = manualConfiguration.Description;
        existing.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ManualConfigurationExists(id))
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

    // DELETE: api/ManualConfigurations/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteManualConfiguration(int id)
    {
        var manualConfiguration = await _context.ManualConfigurations.FindAsync(id);
        if (manualConfiguration == null)
        {
            return NotFound();
        }

        _context.ManualConfigurations.Remove(manualConfiguration);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ManualConfigurationExists(int id)
    {
        return _context.ManualConfigurations.Any(e => e.Id == id);
    }
}
