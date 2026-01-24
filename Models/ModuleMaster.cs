using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MigraTrackAPI.Models;

[Table("ModuleMaster")]
public class ModuleMaster
{
    [Key]
    public int ModuleId { get; set; }

    [Required]
    [MaxLength(200)]
    public string SubModuleName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ModuleName { get; set; } = string.Empty;

    public int? GroupIndex { get; set; }
}
