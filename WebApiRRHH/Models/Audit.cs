using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using NuGet.Protocol.Plugins;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApiRRHH.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebApiRRHH.Models
{
    [Table("Audits")]
    public class Audit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int? UserId { get; set; }

        // Relación con User
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [StringLength(200)]
        public string? UserEmail { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty;

        public string? EntityId { get; set; }

        public string? OldValues { get; set; }
        public string? NewValues { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = "Info";
    }
}
