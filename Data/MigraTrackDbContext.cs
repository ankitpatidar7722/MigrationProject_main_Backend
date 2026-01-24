using Microsoft.EntityFrameworkCore;
using MigraTrackAPI.Models;

namespace MigraTrackAPI.Data;

public class MigraTrackDbContext : DbContext
{
    public MigraTrackDbContext(DbContextOptions<MigraTrackDbContext> options) : base(options)
    {
    }

    // Core Tables
    public DbSet<User> Users { get; set; }
    public DbSet<ExcelData> ExcelData { get; set; } // Pluralized by convention usually, but using singular to match class if preferred, or ExcelDatas
    public DbSet<ModuleGroup> ModuleGroups { get; set; }
    public DbSet<FieldMaster> FieldMaster { get; set; }
    
    // Project Management
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTeamMember> ProjectTeamMembers { get; set; }
    
    // Dynamic Module Data
    public DbSet<DynamicModuleData> DynamicModuleData { get; set; }
    
    // Specific Module Tables
    public DbSet<DataTransferCheck> DataTransferChecks { get; set; }
    public DbSet<VerificationRecord> VerificationRecords { get; set; }
    public DbSet<MigrationIssue> MigrationIssues { get; set; }
    public DbSet<CustomizationPoint> CustomizationPoints { get; set; }
    public DbSet<ProjectEmail> ProjectEmails { get; set; }
    public DbSet<WebTable> WebTables { get; set; }
    
    // Supporting Tables
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<ActivityLog> ActivityLog { get; set; }
    public DbSet<ProjectMilestone> ProjectMilestones { get; set; }
    public DbSet<LookupData> LookupData { get; set; }
    public DbSet<ModuleMaster> ModuleMasters { get; set; }
    public DbSet<ServerData> ServerData { get; set; }
    public DbSet<DatabaseDetail> DatabaseDetails { get; set; }
    public DbSet<ManualConfiguration> ManualConfigurations { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<QuickWork> QuickWorks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure table names to match database schema
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<ModuleGroup>().ToTable("ModuleGroups");
        modelBuilder.Entity<FieldMaster>().ToTable("FieldMaster");
        modelBuilder.Entity<Project>().ToTable("Projects");
        modelBuilder.Entity<ProjectTeamMember>().ToTable("ProjectTeamMembers");
        modelBuilder.Entity<DynamicModuleData>().ToTable("DynamicModuleData");
        modelBuilder.Entity<DataTransferCheck>().ToTable("DataTransferChecks");
        modelBuilder.Entity<VerificationRecord>().ToTable("VerificationRecords");
        modelBuilder.Entity<MigrationIssue>().ToTable("MigrationIssues");
        modelBuilder.Entity<CustomizationPoint>().ToTable("CustomizationPoints");
        modelBuilder.Entity<ProjectEmail>().ToTable("ProjectEmails");
        modelBuilder.Entity<Comment>().ToTable("Comments");
        modelBuilder.Entity<Attachment>().ToTable("Attachments");
        modelBuilder.Entity<ActivityLog>().ToTable("ActivityLog");
        modelBuilder.Entity<ProjectMilestone>().ToTable("ProjectMilestones");
        modelBuilder.Entity<LookupData>().ToTable("LookupData");
        modelBuilder.Entity<ModuleMaster>().ToTable("ModuleMaster");
        modelBuilder.Entity<ServerData>().ToTable("ServerData");
        modelBuilder.Entity<DatabaseDetail>().ToTable("DatabaseDetail");
        modelBuilder.Entity<ManualConfiguration>().ToTable("ManualConfiguration");

        // Configure relationships and constraints
        modelBuilder.Entity<Project>()
            .HasKey(p => p.ProjectId);

        modelBuilder.Entity<DataTransferCheck>()
            .HasOne<Project>()
            .WithMany()
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VerificationRecord>()
            .HasOne<Project>()
            .WithMany()
            .HasForeignKey(v => v.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MigrationIssue>()
            .HasOne<Project>()
            .WithMany()
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomizationPoint>()
            .HasOne<Project>()
            .WithMany()
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExcelData>()
            .HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        modelBuilder.Entity<ProjectEmail>()
            .HasOne<Project>()
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure ManualConfiguration relationship
        modelBuilder.Entity<ManualConfiguration>()
            .HasOne(m => m.Project)
            .WithMany()
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // Configure UserPermission relationship
        modelBuilder.Entity<UserPermission>()
            .HasOne(up => up.User)
            .WithMany()
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        modelBuilder.Entity<UserPermission>()
            .HasIndex(p => new { p.UserId, p.ModuleName })
            .IsUnique();

        // Apply Global Query Filter for parsing Soft Delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "p");
                var deletedProperty = System.Linq.Expressions.Expression.Property(parameter, "IsDeletedTransaction");
                var comparison = System.Linq.Expressions.Expression.Equal(deletedProperty, System.Linq.Expressions.Expression.Constant(0));
                var lambda = System.Linq.Expressions.Expression.Lambda(comparison, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
        {
            switch (entry.State)
            {
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeletedTransaction = 1;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
