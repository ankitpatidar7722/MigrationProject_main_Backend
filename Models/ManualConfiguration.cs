using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MigraTrackAPI.Models;

[Table("ManualConfiguration")]
public class ManualConfiguration
{
    [Key]
    public int Id { get; set; }

    [Required]
    public long ProjectId { get; set; }

    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public Project? Project { get; set; }

    [Required]
    [MaxLength(200)]
    public string ModuleName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string SubModuleName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
