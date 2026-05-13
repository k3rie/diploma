
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Index(IsUnique = true)]
        public string UserName { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(255)]
        public string FullName { get; set; }

        [MaxLength(50)]
        public string Phone { get; set; }

        [MaxLength(255)]
        [Index(IsUnique = true)]
        public string Email { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public int? CompanyId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        public virtual ICollection<Premise> Premises { get; set; }
        public virtual ICollection<Defect> CreatedDefects { get; set; }
        public virtual ICollection<Defect> AssignedDefects { get; set; }
        public virtual ICollection<DefectComment> Comments { get; set; }
        public virtual ICollection<DefectMedia> UploadedMedia { get; set; }
        public virtual ICollection<DefectStatusHistory> StatusChanges { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}