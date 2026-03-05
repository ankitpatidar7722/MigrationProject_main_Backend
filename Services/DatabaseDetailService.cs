using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Services;

public interface IDatabaseDetailService
{
    Task<IEnumerable<DatabaseDetail>> GetAllAsync();
    Task<IEnumerable<DatabaseDetail>> GetByServerIdAsync(int serverId);
    Task<DatabaseDetail?> GetByIdAsync(int id);
    Task<DatabaseDetail> CreateAsync(DatabaseDetail item);
    Task<DatabaseDetail?> UpdateAsync(DatabaseDetail item);
    Task<bool> DeleteAsync(int id);
}

public class DatabaseDetailService : IDatabaseDetailService
{
    private readonly MigraTrackDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(120);

    public DatabaseDetailService(MigraTrackDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private void InvalidateCache()
    {
        _cache.Remove("databases_all");
        // Server-specific caches expire naturally (120s TTL)
    }

    public async Task<IEnumerable<DatabaseDetail>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync("databases_all", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _context.DatabaseDetails
                .AsNoTracking()
                .Include(d => d.Server)
                .ToListAsync();
        }) ?? new List<DatabaseDetail>();
    }

    public async Task<IEnumerable<DatabaseDetail>> GetByServerIdAsync(int serverId)
    {
        var cacheKey = $"databases_server_{serverId}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _context.DatabaseDetails
                .AsNoTracking()
                .Include(d => d.Server)
                .Where(d => d.ServerId == serverId)
                .ToListAsync();
        }) ?? new List<DatabaseDetail>();
    }

    public async Task<DatabaseDetail?> GetByIdAsync(int id)
    {
        return await _context.DatabaseDetails
            .AsNoTracking()
            .Include(d => d.Server)
            .FirstOrDefaultAsync(d => d.DatabaseId == id);
    }

    public async Task<DatabaseDetail> CreateAsync(DatabaseDetail item)
    {
        _context.DatabaseDetails.Add(item);
        await _context.SaveChangesAsync();
        InvalidateCache();
        return item;
    }

    public async Task<DatabaseDetail?> UpdateAsync(DatabaseDetail item)
    {
        var existing = await _context.DatabaseDetails.FindAsync(item.DatabaseId);
        if (existing == null) return null;

        _context.Entry(existing).CurrentValues.SetValues(item);
        await _context.SaveChangesAsync();
        InvalidateCache();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.DatabaseDetails.FindAsync(id);
        if (item == null) return false;

        _context.DatabaseDetails.Remove(item);
        await _context.SaveChangesAsync();
        InvalidateCache();
        return true;
    }
}
