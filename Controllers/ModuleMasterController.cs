using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ModuleMasterController : ControllerBase
{
    private readonly MigraTrackDbContext _context;

    public ModuleMasterController(MigraTrackDbContext context)
    {
        _context = context;
    }

    // GET: api/ModuleMaster
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ModuleMaster>>> GetModuleMasters()
    {
        return await _context.ModuleMasters.OrderBy(m => m.ModuleName).ThenBy(m => m.SubModuleName).ToListAsync();
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

        return NoContent();
    }

    private bool ModuleMasterExists(int id)
    {
        return _context.ModuleMasters.Any(e => e.ModuleId == id);
    }
}
