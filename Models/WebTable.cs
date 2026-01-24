using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MigraTrackAPI.Models;

[Table("WebTables")]
public class WebTable : ISoftDelete
{
    [Key]
    public int WebTableId { get; set; }

    [Required]
    [MaxLength(200)]
    public string TableName { get; set; } = string.Empty;

    public int IsDeletedTransaction { get; set; }

    [MaxLength(200)]
    public string? DesktopTableName { get; set; }

    [MaxLength(200)]
    public string? ModuleName { get; set; }

    public int? GroupIndex { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
