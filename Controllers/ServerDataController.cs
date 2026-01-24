using Microsoft.AspNetCore.Mvc;
using MigraTrackAPI.Models;
using MigraTrackAPI.Services;

namespace MigraTrackAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServerDataController : ControllerBase
{
    private readonly IServerDataService _service;

    public ServerDataController(IServerDataService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServerData>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServerData>> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ServerData>> Create(ServerData item)
    {
        var created = await _service.CreateAsync(item);
        return CreatedAtAction(nameof(Get), new { id = created.ServerId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ServerData item)
    {
        if (id != item.ServerId) return BadRequest();
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
