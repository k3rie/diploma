// Models/DTOs/EngineerDashboardDto.cs
using System.Collections.Generic;

namespace Diploma.Models.DTOs
{
    public class EngineerDashboardDto
    {
        public int TotalDefects { get; set; }
        public Dictionary<string, int> CountByStatus { get; set; }
        public Dictionary<string, int> CountByPriority { get; set; }
        public int OverdueCount { get; set; }
        public double? AverageFixTime { get; set; }
    }
}