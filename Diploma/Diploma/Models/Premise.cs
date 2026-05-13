using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.Models
{
    [Table("Premises")]
    public class Premise
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ObjectId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Number { get; set; }

        [Required]
        public int Floor { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? Area { get; set; }

        public int? OwnerId { get; set; }

        public DateTime? AcceptanceDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Навигационные свойства
        [ForeignKey("ObjectId")]
        public virtual ConstructionObject ConstructionObject { get; set; }

        [ForeignKey("OwnerId")]
        public virtual User Owner { get; set; }

        public virtual ICollection<Defect> Defects { get; set; }
    }
}