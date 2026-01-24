using Microsoft.EntityFrameworkCore;
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

    public ServerDataService(MigraTrackDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ServerData>> GetAllAsync()
    {
        return await _context.ServerData.ToListAsync();
    }

    public async Task<ServerData?> GetByIdAsync(int id)
    {
        return await _context.ServerData.FindAsync(id);
    }

    public async Task<ServerData> CreateAsync(ServerData item)
    {
        _context.ServerData.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<ServerData?> UpdateAsync(ServerData item)
    {
        var existing = await _context.ServerData.FindAsync(item.ServerId);
        if (existing == null) return null;

        _context.Entry(existing).CurrentValues.SetValues(item);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.ServerData.FindAsync(id);
        if (item == null) return false;

        _context.ServerData.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}
