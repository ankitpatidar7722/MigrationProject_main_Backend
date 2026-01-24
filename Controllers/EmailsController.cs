using Microsoft.AspNetCore.Mvc;
using MigraTrackAPI.Models;
using MigraTrackAPI.Services;

namespace MigraTrackAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailsController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailsController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectEmail>>> GetAll()
    {
        var emails = await _emailService.GetAllAsync();
        return Ok(emails);
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<ProjectEmail>>> GetByProject(long projectId)
    {
        var emails = await _emailService.GetByProjectIdAsync(projectId);
        return Ok(emails);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectEmail>> GetById(int id)
    {
        var email = await _emailService.GetByIdAsync(id);
        if (email == null) return NotFound();
        return Ok(email);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectEmail>> Create([FromForm] ProjectEmail email, [FromForm] IFormFile? attachment)
    {
        // Model validation can be tricky with FromForm mixed with object
        // Usually, complex binding works if form fields match property names
        if (!ModelState.IsValid)
        {
            foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Model Error: {modelError.ErrorMessage}");
            }
            return BadRequest(ModelState);
        }

        try 
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "debug_requests.log");
            var receivedLog = $"{DateTime.Now}: Received Create Request: ProjectId={email.ProjectId}, Subject={email.Subject}, Sender={email.Sender}, Receivers={email.Receivers}, BodyLen={email.BodyContent?.Length ?? 0}\n";
            System.IO.File.AppendAllText(logPath, receivedLog);
            
            var created = await _emailService.CreateAsync(email, attachment);
            return CreatedAtAction(nameof(GetById), new { id = created.EmailId }, created);
        }
        catch (Exception ex)
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "debug_errors.log");
            var errorLog = $"{DateTime.Now}: Error: {ex.Message}\nStackTrace: {ex.StackTrace}\nInner: {ex.InnerException?.Message}\n";
            System.IO.File.AppendAllText(logPath, errorLog);
            return StatusCode(500, $"Internal Server Error: {ex.Message} {ex.InnerException?.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ProjectEmail email)
    {
        if (id != email.EmailId) return BadRequest();
        var updated = await _emailService.UpdateAsync(email);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _emailService.DeleteAsync(id);
        if (!result) return NotFound();
        return Ok();
    }
}
