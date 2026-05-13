using Diploma.Core.Enums;
using Diploma.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.Models
{
    [Table("Companies")]
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string LegalAddress { get; set; }

        [MaxLength(20)]
        public string INN { get; set; }

        [MaxLength(20)]
        public string OGRN { get; set; }

        [MaxLength(50)]
        public string Phone { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        public CompanyType CompanyType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Object> Objects { get; set; }
        public virtual ICollection<Defect> Defects { get; set; }
    }
}