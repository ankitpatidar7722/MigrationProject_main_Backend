using Microsoft.AspNetCore.Mvc;
using MigraTrackAPI.Models;
using MigraTrackAPI.Services;

namespace MigraTrackAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetAllProjects()
    {
        var projects = await _projectService.GetAllProjectsAsync();
        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetProject(long id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
            return NotFound();

        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject(Project project)
    {
        try
        {
            // The logic for setting CreatedAt, UpdatedAt, and DisplayOrder
            // should ideally be handled within the service layer.
            // For now, applying the logic directly here as per the instruction,
            // assuming the service method will correctly persist these.
            project.CreatedAt = DateTime.Now;
            project.UpdatedAt = DateTime.Now;

            // The DisplayOrder logic here assumes direct DB context access
            // to calculate max order, which is not available in the controller
            // with IProjectService. This part needs to be handled by the service.
            // For the purpose of applying the change as literally as possible,
            // we'll pass the project with these properties set to the service.
            // The service implementation of CreateProjectAsync would need to
            // incorporate the max order calculation and assignment.

            var created = await _projectService.CreateProjectAsync(project);
            if (created == null)
            {
                return StatusCode(500, "Failed to create project.");
            }
            return CreatedAtAction(nameof(GetProject), new { id = created.ProjectId }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(long id, Project project)
    {
        if (id != project.ProjectId)
            return BadRequest();

        // The provided snippet for Update method directly interacts with _context.
        // To align with the existing service-based architecture, we'll call the service.
        // The service implementation of UpdateProjectAsync would need to handle
        // updating all specified properties and setting UpdatedAt.
        project.UpdatedAt = DateTime.Now; // Set UpdatedAt before passing to service

        var updated = await _projectService.UpdateProjectAsync(project);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(long id)
    {
        var result = await _projectService.DeleteProjectAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id}/dashboard")]
    public async Task<IActionResult> GetProjectDashboard(long id)
    {
        var stats = await _projectService.GetProjectDashboardAsync(id);
        return Ok(stats);
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<Project> projects)
    {
        // This endpoint directly manipulates DisplayOrder.
        // The IProjectService would need a method to handle this.
        // Assuming the service has a method like UpdateProjectDisplayOrdersAsync.
        var success = await _projectService.UpdateProjectDisplayOrdersAsync(projects);
        if (!success)
        {
            return StatusCode(500, "Failed to reorder projects.");
        }
        return Ok();
    }

    [HttpPost("{sourceId}/clone/{targetId}")]
    public async Task<IActionResult> CloneProject(long sourceId, long targetId)
    {
        try
        {
            var success = await _projectService.CloneProjectDataAsync(sourceId, targetId);
            if (!success)
                return BadRequest("Clone failed. Ensure both projects exist.");

            return Ok(new { message = "Project data cloned successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
