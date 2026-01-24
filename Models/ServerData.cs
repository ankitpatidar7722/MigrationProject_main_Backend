using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MigraTrackAPI.Models;

[Table("ServerData")]
public class ServerData
{
    [Key]
    public int ServerId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ServerName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string HostName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ServerIndex { get; set; } = string.Empty;
}
