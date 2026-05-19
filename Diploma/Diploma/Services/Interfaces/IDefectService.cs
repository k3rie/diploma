using Diploma.Models;
using Diploma.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Diploma.Services.Interfaces
{
    public interface IDefectService
    {
        Task<DefectDetailDto> GetDefectDetailsAsync(int defectId, int userId);
        Task<List<DefectListDto>> GetOwnerDefectsAsync(int ownerId);
        Task<List<DefectListDto>> GetPremiseDefectsAsync(int premiseId, int ownerId);
        Task<DefectDetailDto> CreateDefectAsync(DefectCreateDto model, int userId);
        Task UpdateDefectAsync(int defectId, DefectCreateDto model, int userId);
        Task DeleteDefectAsync(int defectId, int userId);
        Task ConfirmDefectFixAsync(int defectId, int userId);
        Task<bool> HasAccessToDefectAsync(int defectId, int userId);
        Task<List<PremiseDto>> GetOwnerPremisesAsync(int ownerId);
        Task<PremiseDto> GetPremiseDetailsAsync(int premiseId);
        Task<OwnerDashboardDto> GetOwnerDashboardAsync(int ownerId);
        // В Diploma.Services.Interfaces.IDefectService инженер
        Task<EngineerDashboardDto> GetEngineerDashboardAsync(int companyId);
        Task<List<DefectListDto>> GetEngineerDefectsAsync(int companyId);
        Task<DefectDetailDto> GetEngineerDefectDetailsAsync(int defectId, int companyId);
        Task AssignDefectAsync(int defectId, int? contractorCompanyId, int? assignedUserId, int changedByUserId, int companyId);
        Task UpdateDefectStatusAsync(int defectId, DefectStatus newStatus, int changedByUserId);
        Task<List<Company>> GetContractorCompaniesAsync();
        Task<List<User>> GetUsersByRoleAndCompanyAsync(UserRole role, int? companyId);
    }

    public class PremiseDto
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int Floor { get; set; }
        public decimal? Area { get; set; }
        public string ObjectName { get; set; }
        public int ObjectId { get; set; }
        public int DefectCount { get; set; }
    }
}