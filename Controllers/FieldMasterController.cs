using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FieldMasterController : ControllerBase
{
    private readonly MigraTrackDbContext _context;

    public FieldMasterController(MigraTrackDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FieldMaster>>> GetAll()
    {
        return Ok(await _context.FieldMaster.Where(f => f.IsActive).ToListAsync());
    }

    [HttpGet("group/{moduleGroupId}")]
    public async Task<ActionResult<IEnumerable<FieldMaster>>> GetByModuleGroup(int moduleGroupId)
    {
        var fields = await _context.FieldMaster
            .Where(f => f.ModuleGroupId == moduleGroupId && f.IsActive)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();
        
        return Ok(fields);
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
        try 
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "debug_field_info.log");
            System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: Creating Field: {field.FieldName}, Group: {field.ModuleGroupId}\n");

            field.CreatedAt = DateTime.Now;
            field.UpdatedAt = DateTime.Now;
            
            _context.FieldMaster.Add(field);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = field.FieldId }, field);
        }
        catch (Exception ex)
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "debug_field_error.log");
            var errorLog = $"{DateTime.Now}: Error: {ex.Message}\nInner: {ex.InnerException?.Message}\nStack: {ex.StackTrace}\n";
            System.IO.File.AppendAllText(logPath, errorLog);
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, FieldMaster field)
    {
        try
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
            
            // New fields
            existing.ValidationRegex = field.ValidationRegex;
            existing.PlaceholderText = field.PlaceholderText;
            existing.HelpText = field.HelpText;
            existing.IsUnique = field.IsUnique;
            existing.MaxLength = field.MaxLength;
            existing.IsDisplay = field.IsDisplay;
            
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }
        catch (Exception ex)
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "debug_field_error.log");
            var errorLog = $"{DateTime.Now}: Update Error: {ex.Message}\nInner: {ex.InnerException?.Message}\nStack: {ex.StackTrace}\n";
            System.IO.File.AppendAllText(logPath, errorLog);
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    [HttpGet("lookup/{type}")]
    public async Task<ActionResult<IEnumerable<object>>> GetLookupValues(string type)
    {
        // Security: Only allow fetching from LookupData table based on type
        // This avoids executing raw SQL from frontend
        return Ok(await _context.LookupData
            .Where(l => l.LookupType == type && l.IsActive)
            .OrderBy(l => l.DisplayOrder)
            .Select(l => new { l.LookupKey, l.LookupValue })
            .ToListAsync());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var field = await _context.FieldMaster.FindAsync(id);
        if (field == null)
            return NotFound();

        field.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
