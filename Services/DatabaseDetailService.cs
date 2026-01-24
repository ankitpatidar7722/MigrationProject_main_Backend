using Microsoft.EntityFrameworkCore;
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

    public DatabaseDetailService(MigraTrackDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DatabaseDetail>> GetAllAsync()
    {
        return await _context.DatabaseDetails
            .Include(d => d.Server)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<DatabaseDetail>> GetByServerIdAsync(int serverId)
    {
        return await _context.DatabaseDetails
            .Include(d => d.Server)
            .Where(d => d.ServerId == serverId)
            .ToListAsync();
    }

    public async Task<DatabaseDetail?> GetByIdAsync(int id)
    {
        return await _context.DatabaseDetails
            .Include(d => d.Server)
            .FirstOrDefaultAsync(d => d.DatabaseId == id);
    }

    public async Task<DatabaseDetail> CreateAsync(DatabaseDetail item)
    {
        _context.DatabaseDetails.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<DatabaseDetail?> UpdateAsync(DatabaseDetail item)
    {
        var existing = await _context.DatabaseDetails.FindAsync(item.DatabaseId);
        if (existing == null) return null;

        _context.Entry(existing).CurrentValues.SetValues(item);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.DatabaseDetails.FindAsync(id);
        if (item == null) return false;

        _context.DatabaseDetails.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}
