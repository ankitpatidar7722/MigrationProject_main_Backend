using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExcelDataController : ControllerBase
{
    private readonly MigraTrackDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ExcelDataController(MigraTrackDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // GET: api/ExcelData/project/5
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<ExcelData>>> GetByProject(long projectId)
    {
        return await _context.ExcelData
            .Where(e => e.ProjectId == projectId)
            .OrderByDescending(e => e.UploadedAt)
            .ToListAsync();
    }

    // POST: api/ExcelData/upload
    [HttpPost("upload")]
    public async Task<ActionResult<ExcelData>> Upload([FromForm] ExcelDataUploadDto uploadDto)
    {
        if (uploadDto.File == null || uploadDto.File.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        // Ensure upload directory exists
        var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "Uploads", "ExcelData");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Generate unique filename
        var uniqueFileName = $"{Guid.NewGuid()}_{uploadDto.File.FileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await uploadDto.File.CopyToAsync(stream);
        }

        var excelData = new ExcelData
        {
            ProjectId = uploadDto.ProjectId,
            ModuleName = uploadDto.ModuleName,
            SubModuleName = uploadDto.SubModuleName,
            Description = uploadDto.Description,
            FileName = uploadDto.File.FileName,
            FilePath = Path.Combine("Uploads", "ExcelData", uniqueFileName).Replace("\\", "/"), // Store relative path
            UploadedBy = uploadDto.UploadedBy,
            UploadedAt = DateTime.UtcNow
        };

        _context.ExcelData.Add(excelData);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByProject), new { projectId = excelData.ProjectId }, excelData);
    }

    // GET: api/ExcelData/download/5
    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(int id)
    {
        var excelData = await _context.ExcelData.FindAsync(id);
        if (excelData == null)
        {
            return NotFound();
        }

        var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, excelData.FilePath);
        
        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound("File not found on server.");
        }

        var memory = new MemoryStream();
        using (var stream = new FileStream(fullPath, FileMode.Open))
        {
            await stream.CopyToAsync(memory);
        }
        memory.Position = 0;

        return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelData.FileName);
    }

    // DELETE: api/ExcelData/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var excelData = await _context.ExcelData.FindAsync(id);
        if (excelData == null)
        {
            return NotFound();
        }

        // Optional: Delete physical file
        try 
        {
            var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, excelData.FilePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            // Log error but continue with DB deletion
            Console.WriteLine($"Error deleting file: {ex.Message}");
        }

        _context.ExcelData.Remove(excelData);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    // PUT: api/ExcelData/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] ExcelDataUploadDto uploadDto)
    {
        var excelData = await _context.ExcelData.FindAsync(id);
        if (excelData == null)
        {
            return NotFound();
        }

        excelData.ModuleName = uploadDto.ModuleName;
        excelData.SubModuleName = uploadDto.SubModuleName;
        excelData.Description = uploadDto.Description;
        excelData.UploadedBy = uploadDto.UploadedBy;

        // Only update file if a new one is provided
        if (uploadDto.File != null && uploadDto.File.Length > 0)
        {
            // Delete old file
            try
            {
                var oldPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, excelData.FilePath);
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting old file: {ex.Message}");
            }

            // Save new file
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "Uploads", "ExcelData");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{uploadDto.File.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await uploadDto.File.CopyToAsync(stream);
            }

            excelData.FileName = uploadDto.File.FileName;
            excelData.FilePath = Path.Combine("Uploads", "ExcelData", uniqueFileName).Replace("\\", "/");
            excelData.UploadedAt = DateTime.UtcNow; // Update timestamp only if file changes? Or always? Let's update it.
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class ExcelDataUploadDto
{
    public long ProjectId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string SubModuleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IFormFile? File { get; set; } // Nullable for updates
    public int? UploadedBy { get; set; }
}
