using Microsoft.AspNetCore.Mvc;
using MigraTrackAPI.Models;
using MigraTrackAPI.Services;

namespace MigraTrackAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataTransferController : ControllerBase
{
    private readonly IDataTransferService _service;

    public DataTransferController(IDataTransferService service)
    {
        _service = service;
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<DataTransferCheck>>> GetByProject(long projectId)
    {
        var items = await _service.GetByProjectIdAsync(projectId);
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DataTransferCheck>> Get(long id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<DataTransferCheck>> Create(DataTransferCheck item)
    {
        var created = await _service.CreateAsync(item);
        return CreatedAtAction(nameof(Get), new { id = created.TransferId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, DataTransferCheck item)
    {
        if (id != item.TransferId)
            return BadRequest();

        var updated = await _service.UpdateAsync(item);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
