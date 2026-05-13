using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.Models
{
    [Table("DefectMedia")]
    public class DefectMedia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int DefectId { get; set; }

        [Required]
        [MaxLength(255)]
        public string OriginalFileName { get; set; }

        [Required]
        [MaxLength(255)]
        public string StoredFileName { get; set; }

        [MaxLength(100)]
        public string ContentType { get; set; }

        public long? FileSize { get; set; }

        [Required]
        public int UploadedByUserId { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public bool IsBeforeFix { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        // Навигационные свойства
        [ForeignKey("DefectId")]
        public virtual Defect Defect { get; set; }

        [ForeignKey("UploadedByUserId")]
        public virtual User UploadedByUser { get; set; }
    }
}