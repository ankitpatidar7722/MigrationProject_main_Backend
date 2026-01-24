using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WebTablesController : ControllerBase
{
    private readonly MigraTrackDbContext _context;

    public WebTablesController(MigraTrackDbContext context)
    {
        _context = context;
    }

    // GET: api/WebTables
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WebTable>>> GetWebTables()
    {
        return await _context.WebTables.OrderBy(w => w.TableName).ToListAsync();
    }

    // GET: api/WebTables/5
    [HttpGet("{id}")]
    public async Task<ActionResult<WebTable>> GetWebTable(int id)
    {
        var webTable = await _context.WebTables.FindAsync(id);

        if (webTable == null)
        {
            return NotFound();
        }

        return webTable;
    }

    // PUT: api/WebTables/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutWebTable(int id, WebTable webTable)
    {
        if (id != webTable.WebTableId)
        {
            return BadRequest();
        }

        _context.Entry(webTable).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!WebTableExists(id))
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

    // POST: api/WebTables
    [HttpPost]
    public async Task<ActionResult<WebTable>> PostWebTable(WebTable webTable)
    {
        _context.WebTables.Add(webTable);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetWebTable", new { id = webTable.WebTableId }, webTable);
    }

    // DELETE: api/WebTables/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWebTable(int id)
    {
        var webTable = await _context.WebTables.FindAsync(id);
        if (webTable == null)
        {
            return NotFound();
        }

        _context.WebTables.Remove(webTable);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool WebTableExists(int id)
    {
        return _context.WebTables.Any(e => e.WebTableId == id);
    }
}
