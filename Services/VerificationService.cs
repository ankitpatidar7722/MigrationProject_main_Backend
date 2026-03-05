using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Services;

public interface IVerificationService
{
    Task<IEnumerable<VerificationRecord>> GetByProjectIdAsync(long projectId);
    Task<VerificationRecord?> GetByIdAsync(long id);
    Task<VerificationRecord> CreateAsync(VerificationRecord item);
    Task<VerificationRecord?> UpdateAsync(VerificationRecord item);
    Task<bool> DeleteAsync(long id);
}

public class VerificationService : IVerificationService
{
    private readonly MigraTrackDbContext _context;
    private readonly IMemoryCache _cache;

    public VerificationService(MigraTrackDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private void InvalidateCache()
    {
        _cache.Remove("dashboard_all");
        _cache.Remove("dashboard_summary");
    }

    public async Task<IEnumerable<VerificationRecord>> GetByProjectIdAsync(long projectId)
    {
        return await _context.VerificationRecords
            .AsNoTracking()
            .Where(v => v.ProjectId == projectId)
            .OrderByDescending(v => v.VerificationId)
            .ToListAsync();
    }

    public async Task<VerificationRecord?> GetByIdAsync(long id)
    {
        return await _context.VerificationRecords.FindAsync(id);
    }

    public async Task<VerificationRecord> CreateAsync(VerificationRecord item)
    {
        _context.VerificationRecords.Add(item);
        await _context.SaveChangesAsync();
        InvalidateCache();
        return item;
    }

    public async Task<VerificationRecord?> UpdateAsync(VerificationRecord item)
    {
        var existing = await _context.VerificationRecords.FindAsync(item.VerificationId);
        if (existing == null) return null;

        _context.Entry(existing).CurrentValues.SetValues(item);
        await _context.SaveChangesAsync();
        InvalidateCache();
        return existing;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var item = await _context.VerificationRecords.FindAsync(id);
        if (item == null) return false;

        _context.VerificationRecords.Remove(item);
        await _context.SaveChangesAsync();
        InvalidateCache();
        return true;
    }
}
