using MigraTrackAPI.Data;
using MigraTrackAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace MigraTrackAPI.Services;

public interface IProjectService
{
    Task<IEnumerable<Project>> GetAllProjectsAsync();
    Task<Project?> GetProjectByIdAsync(long id);
    Task<Project> CreateProjectAsync(Project project);
    Task<Project?> UpdateProjectAsync(Project project);
    Task<bool> DeleteProjectAsync(long id);
    Task<object> GetProjectDashboardAsync(long projectId);
    Task<bool> CloneProjectDataAsync(long sourceProjectId, long targetProjectId);
    Task<bool> UpdateProjectDisplayOrdersAsync(List<Project> projects);
}

public class ProjectService : IProjectService
{
    private readonly MigraTrackDbContext _context;

    public ProjectService(MigraTrackDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        return await _context.Projects
            .Include(p => p.ServerDesktop)
            .Include(p => p.DatabaseDesktop)
            .Include(p => p.ServerWeb)
            .Include(p => p.DatabaseWeb)
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder) // Changed to order by DisplayOrder
            .ThenByDescending(p => p.CreatedAt) // Fallback
            .ToListAsync();
    }

    public async Task<Project?> GetProjectByIdAsync(long id)
    {
        return await _context.Projects
            .Include(p => p.ServerDesktop)
            .Include(p => p.DatabaseDesktop)
            .Include(p => p.ServerWeb)
            .Include(p => p.DatabaseWeb)
            .FirstOrDefaultAsync(p => p.ProjectId == id);
    }

    public async Task<Project> CreateProjectAsync(Project project)
    {
        project.CreatedAt = DateTime.Now;
        project.UpdatedAt = DateTime.Now;
        
        // Auto-increment DisplayOrder logic: Place at TOP (Min - 1)
        var minOrder = await _context.Projects.MinAsync(p => (int?)p.DisplayOrder) ?? 0;
        project.DisplayOrder = minOrder - 1;

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        
        return project;
    }

    public async Task<Project?> UpdateProjectAsync(Project project)
    {
        Console.WriteLine($"[UpdateProjectAsync] Updating Project {project.ProjectId}. ServerIdDesktop: {project.ServerIdDesktop}, DBIdDesktop: {project.DatabaseIdDesktop}");
        var existing = await _context.Projects.FindAsync(project.ProjectId);
        if (existing == null)
            return null;

        existing.ClientName = project.ClientName;
        existing.Description = project.Description;
        existing.Status = project.Status;
        existing.ProjectType = project.ProjectType;
        existing.StartDate = project.StartDate;
        existing.TargetCompletionDate = project.TargetCompletionDate;
        existing.LiveDate = project.LiveDate;
        existing.ProjectManager = project.ProjectManager;
        existing.TechnicalLead = project.TechnicalLead;
        existing.ImplementationCoordinator = project.ImplementationCoordinator;
        existing.CoordinatorEmail = project.CoordinatorEmail;
        // existing.DisplayOrder = project.DisplayOrder; // Do NOT update DisplayOrder on general edit. Preserves position.
        
        // Update Connection Fields
        existing.ServerIdDesktop = project.ServerIdDesktop;
        existing.DatabaseIdDesktop = project.DatabaseIdDesktop;
        existing.ServerIdWeb = project.ServerIdWeb;
        existing.DatabaseIdWeb = project.DatabaseIdWeb;

        existing.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteProjectAsync(long id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null)
            return false;

        project.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<object> GetProjectDashboardAsync(long projectId)
    {
        var totalTransfers = await _context.DataTransferChecks
            .CountAsync(d => d.ProjectId == projectId);

        var completedTransfers = await _context.DataTransferChecks
            .CountAsync(d => d.ProjectId == projectId && d.IsCompleted);

        var totalIssues = await _context.MigrationIssues
            .CountAsync(i => i.ProjectId == projectId && 
                           (i.Status == "Open" || i.Status == "In Progress"));

        var totalVerifications = await _context.VerificationRecords
            .CountAsync(v => v.ProjectId == projectId);

        var completedVerifications = await _context.VerificationRecords
            .CountAsync(v => v.ProjectId == projectId && v.IsVerified);

        return new
        {
            totalModules = totalTransfers + totalVerifications,
            completedMigrations = completedTransfers,
            pendingMigrations = totalTransfers - completedTransfers,
            totalIssues = totalIssues,
            completionPercentage = totalTransfers > 0 
                ? Math.Round((double)completedTransfers / totalTransfers * 100, 2) 
                : 0,
            // Detailed stats
            totalTransfers,
            completedTransfers,
            transferProgress = totalTransfers > 0 
                ? Math.Round((double)completedTransfers / totalTransfers * 100, 2) 
                : 0,
            totalVerifications,
            completedVerifications,
            verificationProgress = totalVerifications > 0 
                ? Math.Round((double)completedVerifications / totalVerifications * 100, 2) 
                : 0
        };
    }

    public async Task<bool> CloneProjectDataAsync(long sourceProjectId, long targetProjectId)
    {
        var sourceProject = await _context.Projects.FindAsync(sourceProjectId);
        var targetProject = await _context.Projects.FindAsync(targetProjectId);

        if (sourceProject == null || targetProject == null)
            return false;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Clone DataTransferChecks
            var checks = await _context.DataTransferChecks
                .Where(x => x.ProjectId == sourceProjectId)
                .AsNoTracking()
                .ToListAsync();
            
            foreach (var check in checks)
            {
                check.TransferId = 0; // Reset ID for new insertion
                check.ProjectId = targetProjectId;
                check.CreatedAt = DateTime.Now;
                check.UpdatedAt = DateTime.Now;
                _context.DataTransferChecks.Add(check);
            }

            // 2. Clone VerificationRecords
            var verifications = await _context.VerificationRecords
                .Where(x => x.ProjectId == sourceProjectId)
                .AsNoTracking()
                .ToListAsync();

            foreach (var ver in verifications)
            {
                ver.VerificationId = 0;
                ver.ProjectId = targetProjectId;
                ver.CreatedAt = DateTime.Now;
                ver.UpdatedAt = DateTime.Now;
                _context.VerificationRecords.Add(ver);
            }

            // 3. Clone CustomizationPoints
            var customizations = await _context.CustomizationPoints
                .Where(x => x.ProjectId == sourceProjectId)
                .AsNoTracking()
                .ToListAsync();

            foreach (var cust in customizations)
            {
                cust.CustomizationId = 0;
                cust.ProjectId = targetProjectId;
                cust.CreatedAt = DateTime.Now;
                cust.UpdatedAt = DateTime.Now;
                _context.CustomizationPoints.Add(cust);
            }

            // 4. Clone MigrationIssues
            var issues = await _context.MigrationIssues
                .Where(x => x.ProjectId == sourceProjectId)
                .AsNoTracking()
                .ToListAsync();

            var timestamp = DateTime.Now.ToString("HHmmss");
            var idx = 0;
            foreach (var issue in issues)
            {
                idx++;
                var newId = $"ISS-{targetProjectId}-{timestamp}-{idx:D3}";
                issue.IssueId = newId;
                issue.IssueNumber = newId;
                issue.ProjectId = targetProjectId;
                issue.CreatedAt = DateTime.Now;
                issue.UpdatedAt = DateTime.Now;
                _context.MigrationIssues.Add(issue);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            // Log exception here conceptually
            throw; 
        }
    }

    public async Task<bool> UpdateProjectDisplayOrdersAsync(List<Project> projects)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var project in projects)
            {
                var existing = await _context.Projects.FindAsync(project.ProjectId);
                if (existing != null)
                {
                    existing.DisplayOrder = project.DisplayOrder;
                }
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
}



// End of existing services
