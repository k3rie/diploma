using Diploma.Models;
using Diploma.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Diploma.Data.Interfaces
{
    public interface IDefectRepository
    {
        Task<Defect> GetByIdAsync(int id);
        Task<Defect> GetByIdWithDetailsAsync(int id);
        Task<List<Defect>> GetByOwnerIdAsync(int ownerId);
        Task<List<Defect>> GetByPremiseIdAsync(int premiseId);
        Task<Defect> CreateAsync(Defect defect);
        Task UpdateAsync(Defect defect);
        Task SoftDeleteAsync(int id);
        Task<bool> HasAccessToDefectAsync(int defectId, int userId);
        Task<List<Premise>> GetOwnerPremisesAsync(int ownerId);
        Task<Premise> GetPremiseByIdAsync(int premiseId);
        Task AddCommentAsync(DefectComment comment);
        Task AddMediaAsync(DefectMedia media);
        Task UpdateStatusAsync(int defectId, DefectStatus newStatus, int changedByUserId, string comment = null);
        Task<OwnerDashboardDto> GetOwnerDashboardDataAsync(int ownerId);
    }
}