using System.ComponentModel.DataAnnotations;

namespace MigraTrackAPI.Models;

public class VerificationRecord : ISoftDelete
{
    [Key]
    public long VerificationId { get; set; }

    [Required]
    public long ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ModuleName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? SubModuleName { get; set; }

    [Required]
    [MaxLength(200)]
    public string FieldName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? TableNameDesktop { get; set; }

    [MaxLength(200)]
    public string? TableNameWeb { get; set; }

    public string? Description { get; set; }

    public string? SqlQuery { get; set; }

    public string? ExpectedResult { get; set; }

    public string? ActualResult { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    public bool IsVerified { get; set; } = false;

    public int IsDeletedTransaction { get; set; }

    [MaxLength(200)]
    public string? VerifiedBy { get; set; }

    public DateTime? VerifiedDate { get; set; }

    public string? Comments { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
