using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MigraTrackAPI.Models
{
    public class UserPermission
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ModuleName { get; set; } = string.Empty;

        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanSave { get; set; }
        public bool CanDelete { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
