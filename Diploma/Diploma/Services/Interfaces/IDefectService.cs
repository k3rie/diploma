using Diploma.Models;
using Diploma.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

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
        Task MarkDefectFixedWithPhotosAsync(int defectId, int userId, IEnumerable<HttpPostedFileBase> photos);
        // Получение списка дефектов подрядчика
        Task<List<DefectListDto>> GetContractorDefectsAsync(int userId);
        Task<DefectDetailDto> GetDefectDetailForContractorAsync(int defectId, int userId);
        Task AddCommentAsync(int defectId, int userId, string message);
        Task RejectFixAsync(int defectId, int userId, string comment);
        Task<List<DefectListDto>> GetAllDefectsAsync();
        Task<List<DefectListDto>> GetFilteredDefectsAsync(int? companyId = null, int? objectId = null, int? premiseId = null, DefectStatus? status = null);
        Task<AdminDashboardDto> GetAdminDashboardAsync(int? companyId = null, int? objectId = null, int? status = null);

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