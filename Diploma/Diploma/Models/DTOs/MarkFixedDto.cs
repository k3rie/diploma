using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Diploma.Models.DTOs
{
    public class MarkFixedDto
    {
        public int DefectId { get; set; }
        public List<HttpPostedFileBase> Photos { get; set; }
    }
}