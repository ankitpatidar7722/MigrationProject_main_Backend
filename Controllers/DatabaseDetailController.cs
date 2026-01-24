using Microsoft.AspNetCore.Mvc;
using MigraTrackAPI.Models;
using MigraTrackAPI.Services;

namespace MigraTrackAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseDetailController : ControllerBase
{
    private readonly IDatabaseDetailService _service;

    public DatabaseDetailController(IDatabaseDetailService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DatabaseDetail>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("server/{serverId}")]
    public async Task<ActionResult<IEnumerable<DatabaseDetail>>> GetByServer(int serverId)
    {
        return Ok(await _service.GetByServerIdAsync(serverId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DatabaseDetail>> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<DatabaseDetail>> Create(DatabaseDetail item)
    {
        var created = await _service.CreateAsync(item);
        return CreatedAtAction(nameof(Get), new { id = created.DatabaseId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, DatabaseDetail item)
    {
        if (id != item.DatabaseId) return BadRequest();
        var updated = await _service.UpdateAsync(item);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
