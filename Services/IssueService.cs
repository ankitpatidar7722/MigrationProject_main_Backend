using Microsoft.EntityFrameworkCore;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Services;

public interface IIssueService
{
    Task<IEnumerable<MigrationIssue>> GetByProjectIdAsync(long projectId);
    Task<MigrationIssue?> GetByIdAsync(string id);
    Task<MigrationIssue> CreateAsync(MigrationIssue item);
    Task<MigrationIssue?> UpdateAsync(MigrationIssue item);
    Task<bool> DeleteAsync(string id);
}

public class IssueService : IIssueService
{
    private readonly MigraTrackDbContext _context;

    public IssueService(MigraTrackDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MigrationIssue>> GetByProjectIdAsync(long projectId)
    {
        return await _context.MigrationIssues
            .Where(i => i.ProjectId == projectId)
            .OrderByDescending(i => i.IssueId) // Or ReportedDate if available
            .ToListAsync();
    }

    public async Task<MigrationIssue?> GetByIdAsync(string id)
    {
        return await _context.MigrationIssues.FindAsync(id);
    }

    public async Task<MigrationIssue> CreateAsync(MigrationIssue item)
    {
        _context.MigrationIssues.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<MigrationIssue?> UpdateAsync(MigrationIssue item)
    {
        var existing = await _context.MigrationIssues.FindAsync(item.IssueId);
        if (existing == null) return null;

        _context.Entry(existing).CurrentValues.SetValues(item);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var item = await _context.MigrationIssues.FindAsync(id);
        if (item == null) return false;

        _context.MigrationIssues.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}
