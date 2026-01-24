using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MigraTrackAPI.Models;

[Table("CustomizationPoints")]
public class CustomizationPoint : ISoftDelete
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long CustomizationId { get; set; }

    [MaxLength(50)]
    public string? RequirementId { get; set; }

    [Required]
    public long ProjectId { get; set; }

    [MaxLength(200)]
    public string? ModuleName { get; set; }

    [MaxLength(200)]
    public string? SubModuleName { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Not Started";

    public bool IsBillable { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? EstimatedCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ActualCost { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? EstimatedHours { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? ActualHours { get; set; }

    [MaxLength(50)]
    public string? Priority { get; set; }

    [MaxLength(200)]
    public string? RequestedBy { get; set; }

    [MaxLength(200)]
    public string? ApprovedBy { get; set; }

    [MaxLength(200)]
    public string? DevelopedBy { get; set; }

    public DateTime? RequestedDate { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public string? Notes { get; set; }

    public int IsDeletedTransaction { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
