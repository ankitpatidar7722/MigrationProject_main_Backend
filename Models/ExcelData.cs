using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MigraTrackAPI.Models;

public class ExcelData
{
    [Key]
    public int Id { get; set; }

    [Required]
    public long ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ModuleName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string SubModuleName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    public int? UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ProjectId")]
    public Project? Project { get; set; }
}
