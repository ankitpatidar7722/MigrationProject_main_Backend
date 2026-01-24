using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MigraTrackAPI.Models;

public class DataTransferCheck : ISoftDelete
{
    [Key]
    public long TransferId { get; set; }

    public bool IsTransferSuccessful { get; set; } = false;

    public int IsDeletedTransaction { get; set; }

    [Required]
    public long ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ModuleName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? SubModuleName { get; set; }

    [MaxLength(500)]
    public string? Condition { get; set; }

    [MaxLength(200)]
    public string? TableNameDesktop { get; set; }

    [Required]
    [MaxLength(200)]
    public string TableNameWeb { get; set; } = string.Empty;

    public long? RecordCountDesktop { get; set; }

    public long? RecordCountWeb { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? MatchPercentage { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Not Started";

    public bool IsCompleted { get; set; }

    public DateTime? MigratedDate { get; set; }

    [MaxLength(200)]
    public string? VerifiedBy { get; set; }

    public string? Comments { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
