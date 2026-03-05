using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Services;

public interface IIssueService
{
    Task<IEnumerable<MigrationIssue>> GetAllAsync();
    Task<IEnumerable<MigrationIssue>> GetByProjectIdAsync(long projectId);
    Task<MigrationIssue?> GetByIdAsync(string id);
    Task<MigrationIssue> CreateAsync(MigrationIssue item);
    Task<MigrationIssue?> UpdateAsync(MigrationIssue item);
    Task<bool> DeleteAsync(string id);
}

public class IssueService : IIssueService
{
    private readonly MigraTrackDbContext _context;
    private readonly IMemoryCache _cache;

    public IssueService(MigraTrackDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private void InvalidateCache()
    {
        _cache.Remove("dashboard_all");
        _cache.Remove("dashboard_summary");
    }

    public async Task<IEnumerable<MigrationIssue>> GetAllAsync()
    {
        return await _context.MigrationIssues
            .AsNoTracking()
            .OrderByDescending(i => i.ReportedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<MigrationIssue>> GetByProjectIdAsync(long projectId)
    {
        return await _context.MigrationIssues
            .AsNoTracking()
            .Where(i => i.ProjectId == projectId)
            .OrderByDescending(i => i.ReportedDate)
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
        InvalidateCache();
        return item;
    }

    public async Task<MigrationIssue?> UpdateAsync(MigrationIssue item)
    {
        var existing = await _context.MigrationIssues.FindAsync(item.IssueId);
        if (existing == null) return null;

        _context.Entry(existing).CurrentValues.SetValues(item);
        await _context.SaveChangesAsync();
        InvalidateCache();
        return existing;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var item = await _context.MigrationIssues.FindAsync(id);
        if (item == null) return false;

        _context.MigrationIssues.Remove(item);
        await _context.SaveChangesAsync();
        InvalidateCache();
        return true;
    }
}
