using Microsoft.AspNetCore.Mvc;
using MigraTrackAPI.Models;
using MigraTrackAPI.Services;

namespace MigraTrackAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IssuesController : ControllerBase
{
    private readonly IIssueService _service;

    public IssuesController(IIssueService service)
    {
        _service = service;
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<MigrationIssue>>> GetByProject(long projectId)
    {
        var items = await _service.GetByProjectIdAsync(projectId);
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MigrationIssue>> Get(string id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<MigrationIssue>> Create(MigrationIssue item)
    {
        var created = await _service.CreateAsync(item);
        return CreatedAtAction(nameof(Get), new { id = created.IssueId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, MigrationIssue item)
    {
        if (id != item.IssueId)
            return BadRequest();

        var updated = await _service.UpdateAsync(item);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
