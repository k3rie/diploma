using System.Collections.Generic;

namespace Diploma.Models.DTOs
{
    public class OwnerDashboardDto
    {
        public int TotalDefects { get; set; }
        public Dictionary<string, int> CountByStatus { get; set; }
        public Dictionary<string, int> CountByPriority { get; set; }
        public List<DefectListDto> RecentDefects { get; set; }
    }
}