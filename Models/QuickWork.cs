using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MigraTrackAPI.Models
{
    public class QuickWork
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(200)]
        public string? ModuleName { get; set; }

        [MaxLength(200)]
        public string? SubModuleName { get; set; }

        [MaxLength(200)]
        public string? TableName { get; set; }

        public string? Description { get; set; }

        public string? SqlQuery { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
