using Diploma.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Diploma.Models
{
    public class AdminIndexViewModel
    {
        public AdminDashboardDto Dashboard { get; set; }
        public List<DefectListDto> Defects { get; set; }
    }
}