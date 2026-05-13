
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.Models
{
    [Table("Defects")]
    public class Defect
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DefectStatus Status { get; set; } = DefectStatus.Created;

        [Required]
        public DefectPriority Priority { get; set; } = DefectPriority.Medium;

        [Required]
        public int PremisesId { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }

        public int? AssignedToUserId { get; set; }

        public int? ContractorCompanyId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? ClosedAt { get; set; }

        public DateTime? AcceptedByOwnerAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Навигационные свойства
        [ForeignKey("PremisesId")]
        public virtual Premise Premise { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; }

        [ForeignKey("AssignedToUserId")]
        public virtual User AssignedToUser { get; set; }

        [ForeignKey("ContractorCompanyId")]
        public virtual Company ContractorCompany { get; set; }

        public virtual ICollection<DefectComment> Comments { get; set; }
        public virtual ICollection<DefectMedia> MediaFiles { get; set; }
        public virtual ICollection<DefectStatusHistory> StatusHistory { get; set; }
    }
}