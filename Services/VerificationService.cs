using Microsoft.EntityFrameworkCore;
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

    public VerificationService(MigraTrackDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VerificationRecord>> GetByProjectIdAsync(long projectId)
    {
        return await _context.VerificationRecords
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
        return item;
    }

    public async Task<VerificationRecord?> UpdateAsync(VerificationRecord item)
    {
        var existing = await _context.VerificationRecords.FindAsync(item.VerificationId);
        if (existing == null) return null;

        _context.Entry(existing).CurrentValues.SetValues(item);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var item = await _context.VerificationRecords.FindAsync(id);
        if (item == null) return false;

        _context.VerificationRecords.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}
