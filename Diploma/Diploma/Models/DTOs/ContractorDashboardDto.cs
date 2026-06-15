using System.Collections.Generic;

namespace Diploma.Models.DTOs
{
    public class ContractorDashboardDto
    {
        public int TotalDefects { get; set; }
        public Dictionary<string, int> CountByStatus { get; set; }
    }
}