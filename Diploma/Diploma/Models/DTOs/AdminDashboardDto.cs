using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Diploma.Models.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalDefects { get; set; }
        public Dictionary<string, int> CountByStatus { get; set; }
        public int OverdueCount { get; set; }
        public double? AverageFixTime { get; set; }
    }
}