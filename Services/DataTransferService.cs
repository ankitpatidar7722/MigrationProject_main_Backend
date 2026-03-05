using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Services;

public interface IDataTransferService
{
    Task<IEnumerable<DataTransferCheck>> GetAllAsync();
    Task<IEnumerable<DataTransferCheck>> GetByProjectIdAsync(long projectId);
    Task<DataTransferCheck?> GetByIdAsync(long id);
    Task<DataTransferCheck> CreateAsync(DataTransferCheck item);
    Task<DataTransferCheck?> UpdateAsync(DataTransferCheck item);
    Task<bool> DeleteAsync(long id);
}

public class DataTransferService : IDataTransferService
{
    private readonly MigraTrackDbContext _context;
    private readonly IMemoryCache _cache;

    public DataTransferService(MigraTrackDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private void InvalidateCache()
    {
        _cache.Remove("dashboard_all");
        _cache.Remove("dashboard_summary");
    }

    public async Task<IEnumerable<DataTransferCheck>> GetAllAsync()
    {
        return await _context.DataTransferChecks
            .AsNoTracking()
            .OrderByDescending(d => d.TransferId)
            .ToListAsync();
    }

    public async Task<IEnumerable<DataTransferCheck>> GetByProjectIdAsync(long projectId)
    {
        return await _context.DataTransferChecks
            .AsNoTracking()
            .Where(d => d.ProjectId == projectId)
            .OrderByDescending(d => d.TransferId)
            .ToListAsync();
    }

    public async Task<DataTransferCheck?> GetByIdAsync(long id)
    {
        return await _context.DataTransferChecks.FindAsync(id);
    }

    public async Task<DataTransferCheck> CreateAsync(DataTransferCheck item)
    {
        _context.DataTransferChecks.Add(item);
        await _context.SaveChangesAsync();
        InvalidateCache();
        return item;
    }

    public async Task<DataTransferCheck?> UpdateAsync(DataTransferCheck item)
    {
        var existing = await _context.DataTransferChecks.FindAsync(item.TransferId);
        if (existing == null) return null;

        _context.Entry(existing).CurrentValues.SetValues(item);
        await _context.SaveChangesAsync();
        InvalidateCache();
        return existing;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var item = await _context.DataTransferChecks.FindAsync(id);
        if (item == null) return false;

        _context.DataTransferChecks.Remove(item);
        await _context.SaveChangesAsync();
        InvalidateCache();
        return true;
    }
}
