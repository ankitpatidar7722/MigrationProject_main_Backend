using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MigraTrackAPI.Models;

public class MigrationIssue : ISoftDelete
{
    [Key]
    [MaxLength(50)]
    public string IssueId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? IssueNumber { get; set; }

    [Required]
    public long ProjectId { get; set; }

    public bool IsResolved { get; set; } = false;

    public int IsDeletedTransaction { get; set; }

    [MaxLength(200)]
    public string? ModuleName { get; set; }

    [MaxLength(200)]
    public string? SubModuleName { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? RootCause { get; set; }

    public string? Solution { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Open";

    [Required]
    [MaxLength(50)]
    public string Priority { get; set; } = "Medium";

    [MaxLength(50)]
    public string? Severity { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(200)]
    public string? AssignedTo { get; set; }

    [MaxLength(200)]
    public string? ReportedBy { get; set; }

    [Required]
    public DateTime ReportedDate { get; set; } = DateTime.Now;

    public DateTime? ResolvedDate { get; set; }

    public DateTime? ClosedDate { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? EstimatedHours { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? ActualHours { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
