using System.ComponentModel.DataAnnotations;

namespace MigraTrackAPI.Models
{
    public class User : ISoftDelete
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? PasswordHash { get; set; } // Matches database column name

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string Role { get; set; } = "Admin";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int IsDeletedTransaction { get; set; }

        public virtual ICollection<UserPermission> Permissions { get; set; } = new List<UserPermission>();
    }
}
