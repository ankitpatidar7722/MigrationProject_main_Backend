using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MigraTrackAPI.Models;

[Table("DatabaseDetail")]
public class DatabaseDetail
{
    [Key]
    public int DatabaseId { get; set; }

    [Required]
    [MaxLength(200)]
    public string DatabaseName { get; set; } = string.Empty;

    [Required]
    public int ServerId { get; set; }

    [ForeignKey("ServerId")]
    public ServerData? Server { get; set; }

    [Required]
    [MaxLength(50)]
    public string ServerIndex { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ClientName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? DatabaseCategory { get; set; }
}
