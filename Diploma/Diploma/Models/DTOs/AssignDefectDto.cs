using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Diploma.Models.DTOs
{
    public class AssignDefectDto
    {
        public int DefectId { get; set; }
        public int? ContractorCompanyId { get; set; }
        public int? AssignedToUserId { get; set; }
        public bool AssignToMe { get; set; }
    }
}