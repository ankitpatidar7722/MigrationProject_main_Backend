using MigraTrackAPI.Data;
using MigraTrackAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MigraTrackAPI.Services;

public interface IEmailService
{
    Task<IEnumerable<ProjectEmail>> GetAllAsync();
    Task<IEnumerable<ProjectEmail>> GetByProjectIdAsync(long projectId);
    Task<ProjectEmail?> GetByIdAsync(int id);
    Task<ProjectEmail> CreateAsync(ProjectEmail email, IFormFile? attachment);
    Task<ProjectEmail?> UpdateAsync(ProjectEmail email);
    Task<bool> DeleteAsync(int id);
}

public class EmailService : IEmailService
{
    private readonly MigraTrackDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public EmailService(MigraTrackDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IEnumerable<ProjectEmail>> GetAllAsync()
    {
        return await _context.ProjectEmails
            .OrderByDescending(e => e.EmailDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectEmail>> GetByProjectIdAsync(long projectId)
    {
        return await _context.ProjectEmails
            .Where(e => e.ProjectId == projectId)
            .OrderByDescending(e => e.EmailDate)
            .ToListAsync();
    }

    public async Task<ProjectEmail?> GetByIdAsync(int id)
    {
        return await _context.ProjectEmails.FindAsync(id);
    }

    public async Task<ProjectEmail> CreateAsync(ProjectEmail email, IFormFile? attachment)
    {
        if (attachment != null && attachment.Length > 0)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "emails");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{attachment.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await attachment.CopyToAsync(fileStream);
            }

            email.AttachmentPath = $"/uploads/emails/{uniqueFileName}";
        }

        email.CreatedAt = DateTime.Now;
        email.UpdatedAt = DateTime.Now;

        _context.ProjectEmails.Add(email);
        await _context.SaveChangesAsync();

        return email;
    }

    public async Task<ProjectEmail?> UpdateAsync(ProjectEmail email)
    {
        var existing = await _context.ProjectEmails.FindAsync(email.EmailId);
        if (existing == null) return null;

        existing.Subject = email.Subject;
        existing.Sender = email.Sender;
        existing.Receivers = email.Receivers;
        existing.EmailDate = email.EmailDate;
        existing.BodyContent = email.BodyContent;
        existing.Category = email.Category;
        existing.RelatedModule = email.RelatedModule;
        existing.UpdatedAt = DateTime.Now;

        // Attachment update logic usually handled separately or requires re-upload
        // For simple update, assuming we don't clear attachment unless specific logic exists

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var email = await _context.ProjectEmails.FindAsync(id);
        if (email == null) return false;

        // OPTIONAL: Delete physical file if exists
        // if (!string.IsNullOrEmpty(email.AttachmentPath)) { ... }

        _context.ProjectEmails.Remove(email);
        await _context.SaveChangesAsync();
        return true;
    }
}
