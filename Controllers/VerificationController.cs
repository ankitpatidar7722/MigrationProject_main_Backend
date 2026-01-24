using Microsoft.AspNetCore.Mvc;
using MigraTrackAPI.Models;
using MigraTrackAPI.Services;

namespace MigraTrackAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VerificationController : ControllerBase
{
    private readonly IVerificationService _service;

    public VerificationController(IVerificationService service)
    {
        _service = service;
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<VerificationRecord>>> GetByProject(long projectId)
    {
        var items = await _service.GetByProjectIdAsync(projectId);
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VerificationRecord>> Get(long id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<VerificationRecord>> Create(VerificationRecord item)
    {
        var created = await _service.CreateAsync(item);
        return CreatedAtAction(nameof(Get), new { id = created.VerificationId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, VerificationRecord item)
    {
        if (id != item.VerificationId)
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
