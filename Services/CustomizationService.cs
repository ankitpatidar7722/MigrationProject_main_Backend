using Microsoft.EntityFrameworkCore;
using MigraTrackAPI.Data;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Services;

public interface ICustomizationService
{
    Task<IEnumerable<CustomizationPoint>> GetByProjectIdAsync(long projectId);
    Task<CustomizationPoint?> GetByIdAsync(long id);
    Task<CustomizationPoint> CreateAsync(CustomizationPoint item);
    Task<CustomizationPoint?> UpdateAsync(CustomizationPoint item);
    Task<bool> DeleteAsync(long id);
}

public class CustomizationService : ICustomizationService
{
    private readonly MigraTrackDbContext _context;

    public CustomizationService(MigraTrackDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomizationPoint>> GetByProjectIdAsync(long projectId)
    {
        return await _context.CustomizationPoints
            .Where(c => c.ProjectId == projectId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<CustomizationPoint?> GetByIdAsync(long id)
    {
        return await _context.CustomizationPoints.FindAsync(id);
    }

    public async Task<CustomizationPoint> CreateAsync(CustomizationPoint item)
    {
        _context.CustomizationPoints.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<CustomizationPoint?> UpdateAsync(CustomizationPoint item)
    {
        var existing = await _context.CustomizationPoints.FindAsync(item.CustomizationId);
        if (existing == null) return null;

        _context.Entry(existing).CurrentValues.SetValues(item);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var item = await _context.CustomizationPoints.FindAsync(id);
        if (item == null) return false;

        _context.CustomizationPoints.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}
