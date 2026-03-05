using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Services;

public interface IServerDataService
{
    Task<IEnumerable<ServerData>> GetAllAsync();
    Task<ServerData?> GetByIdAsync(int id);
    Task<ServerData> CreateAsync(ServerData item);
    Task<ServerData?> UpdateAsync(ServerData item);
    Task<bool> DeleteAsync(int id);
}

public class ServerDataService : IServerDataService
{
    private readonly MigraTrackDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(120);

    public ServerDataService(MigraTrackDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private void InvalidateCache()
    {
        _cache.Remove("servers_all");
        _cache.Remove("databases_all"); // Also invalidate databases since they include Server nav prop
    }

    public async Task<IEnumerable<ServerData>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync("servers_all", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _context.ServerData.AsNoTracking().ToListAsync();
        }) ?? new List<ServerData>();
    }

    public async Task<ServerData?> GetByIdAsync(int id)
    {
        return await _context.ServerData.FindAsync(id);
    }

    public async Task<ServerData> CreateAsync(ServerData item)
    {
        _context.ServerData.Add(item);
        await _context.SaveChangesAsync();
        InvalidateCache();
        return item;
    }

    public async Task<ServerData?> UpdateAsync(ServerData item)
    {
        var existing = await _context.ServerData.FindAsync(item.ServerId);
        if (existing == null) return null;

        _context.Entry(existing).CurrentValues.SetValues(item);
        await _context.SaveChangesAsync();
        InvalidateCache();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.ServerData.FindAsync(id);
        if (item == null) return false;

        _context.ServerData.Remove(item);
        await _context.SaveChangesAsync();
        InvalidateCache();
        return true;
    }
}
