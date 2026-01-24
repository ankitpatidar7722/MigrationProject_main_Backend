using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MigraTrackAPI.Models;

public class ProjectEmail : ISoftDelete
{
    [Key]
    public int EmailId { get; set; }

    [Required]
    public long ProjectId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Sender { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Receivers { get; set; } = string.Empty;

    [Required]
    public DateTime EmailDate { get; set; }

    public string BodyContent { get; set; } = string.Empty; // Full email text

    [MaxLength(50)]
    public string Category { get; set; } = "General"; // Approval, Clarification, etc.

    public string? AttachmentPath { get; set; } // Path to uploaded file

    [MaxLength(100)]
    public string? RelatedModule { get; set; } // Optional link to other modules

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int IsDeletedTransaction { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
