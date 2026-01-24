using Microsoft.EntityFrameworkCore;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Services;

public interface IDataTransferService
{
    Task<IEnumerable<DataTransferCheck>> GetByProjectIdAsync(long projectId);
    Task<DataTransferCheck?> GetByIdAsync(long id);
    Task<DataTransferCheck> CreateAsync(DataTransferCheck item);
    Task<DataTransferCheck?> UpdateAsync(DataTransferCheck item);
    Task<bool> DeleteAsync(long id);
}

public class DataTransferService : IDataTransferService
{
    private readonly MigraTrackDbContext _context;

    public DataTransferService(MigraTrackDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DataTransferCheck>> GetByProjectIdAsync(long projectId)
    {
        return await _context.DataTransferChecks
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
        return item;
    }

    public async Task<DataTransferCheck?> UpdateAsync(DataTransferCheck item)
    {
        var existing = await _context.DataTransferChecks.FindAsync(item.TransferId);
        if (existing == null) return null;

        _context.Entry(existing).CurrentValues.SetValues(item);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var item = await _context.DataTransferChecks.FindAsync(id);
        if (item == null) return false;

        _context.DataTransferChecks.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}
