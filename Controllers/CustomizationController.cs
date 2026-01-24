using Microsoft.AspNetCore.Mvc;
using MigraTrackAPI.Models;
using MigraTrackAPI.Services;

namespace MigraTrackAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomizationController : ControllerBase
{
    private readonly ICustomizationService _service;

    public CustomizationController(ICustomizationService service)
    {
        _service = service;
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<CustomizationPoint>>> GetByProject(long projectId)
    {
        var items = await _service.GetByProjectIdAsync(projectId);
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomizationPoint>> Get(long id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<CustomizationPoint>> Create(CustomizationPoint item)
    {
        try
        {
            var created = await _service.CreateAsync(item);
            return CreatedAtAction(nameof(Get), new { id = created.CustomizationId }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message, innerException = ex.InnerException?.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, CustomizationPoint item)
    {
        if (id != item.CustomizationId)
            return BadRequest();

        var updated = await _service.UpdateAsync(item);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound();
            
        return NoContent();
    }
}
