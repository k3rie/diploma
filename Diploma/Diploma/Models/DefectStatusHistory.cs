using Diploma.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.Models
{
    [Table("DefectStatusHistory")]
    public class DefectStatusHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int DefectId { get; set; }

        [Required]
        public DefectStatus OldStatus { get; set; }

        [Required]
        public DefectStatus NewStatus { get; set; }

        [Required]
        public int ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.Now;

        public string Comment { get; set; }

        // Навигационные свойства
        [ForeignKey("DefectId")]
        public virtual Defect Defect { get; set; }

        [ForeignKey("ChangedByUserId")]
        public virtual User ChangedByUser { get; set; }
    }
}