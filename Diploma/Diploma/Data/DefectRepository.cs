using Diploma.Data.Interfaces;
using Diploma.Models;
using Diploma.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Diploma.Data
{
    public class DefectRepository : IDefectRepository
    {
        private readonly DefectDbContext _context;

        public DefectRepository(DefectDbContext context)
        {
            _context = context;
        }

        public async Task<Defect> GetByIdAsync(int id)
        {
            return await _context.Defects
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        public async Task<Defect> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Defects
                .Include(d => d.Premise.ConstructionObject)
                .Include(d => d.CreatedByUser)
                .Include(d => d.AssignedToUser)
                .Include(d => d.ContractorCompany)
                .Include(d => d.Comments.Select(c => c.User))
                .Include(d => d.MediaFiles)
                .Include(d => d.StatusHistory.Select(h => h.ChangedByUser))
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        public async Task<List<Defect>> GetByOwnerIdAsync(int ownerId)
        {
            return await _context.Defects
                .Include(d => d.Premise.ConstructionObject)
                .Include(d => d.Comments)
                .Include(d => d.MediaFiles)
                .Where(d => !d.IsDeleted && d.CreatedByUserId == ownerId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Defect>> GetByPremiseIdAsync(int premiseId)
        {
            return await _context.Defects
                .Where(d => d.PremisesId == premiseId && !d.IsDeleted)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<Defect> CreateAsync(Defect defect)
        {
            defect.CreatedAt = DateTime.Now;
            defect.Status = DefectStatus.Created;
            defect.IsDeleted = false;

            _context.Defects.Add(defect);
            await _context.SaveChangesAsync();

            // Add initial status history
            var statusHistory = new DefectStatusHistory
            {
                DefectId = defect.Id,
                OldStatus = DefectStatus.Created,
                NewStatus = DefectStatus.Created,
                ChangedByUserId = defect.CreatedByUserId,
                ChangedAt = DateTime.Now,
                Comment = "Дефект создан"
            };

            _context.DefectStatusHistory.Add(statusHistory);
            await _context.SaveChangesAsync();

            return defect;
        }

        public async Task UpdateAsync(Defect defect)
        {
            defect.UpdatedAt = DateTime.Now;
            _context.Entry(defect).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            var defect = await _context.Defects.FindAsync(id);
            if (defect != null)
            {
                defect.IsDeleted = true;
                defect.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasAccessToDefectAsync(int defectId, int userId)
        {
            return await _context.Defects
                .AnyAsync(d => d.Id == defectId && d.CreatedByUserId == userId && !d.IsDeleted);
        }

        public async Task<List<Premise>> GetOwnerPremisesAsync(int ownerId)
        {
            return await _context.Premises
                .Include(p => p.ConstructionObject)
                .Where(p => p.OwnerId == ownerId)
                .OrderBy(p => p.ConstructionObject.Name)
                .ThenBy(p => p.Floor)
                .ThenBy(p => p.Number)
                .ToListAsync();
        }

        public async Task<Premise> GetPremiseByIdAsync(int premiseId)
        {
            return await _context.Premises
                .Include(p => p.ConstructionObject)
                .FirstOrDefaultAsync(p => p.Id == premiseId);
        }

        public async Task AddCommentAsync(DefectComment comment)
        {
            comment.CreatedAt = DateTime.Now;
            _context.DefectComments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task AddMediaAsync(DefectMedia media)
        {
            media.UploadedAt = DateTime.Now;
            _context.DefectMedia.Add(media);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int defectId, DefectStatus newStatus, int changedByUserId, string comment = null)
        {
            var defect = await _context.Defects.FindAsync(defectId);
            if (defect == null) return;

            var oldStatus = defect.Status;
            defect.Status = newStatus;
            defect.UpdatedAt = DateTime.Now;

            if (newStatus == DefectStatus.Confirmed)
            {
                defect.AcceptedByOwnerAt = DateTime.Now;
                defect.ClosedAt = DateTime.Now;
            }

            var statusHistory = new DefectStatusHistory
            {
                DefectId = defectId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedByUserId = changedByUserId,
                ChangedAt = DateTime.Now,
                Comment = comment ?? $"Status changed from {oldStatus} to {newStatus}"
            };

            _context.DefectStatusHistory.Add(statusHistory);
            await _context.SaveChangesAsync();
        }
        public async Task<OwnerDashboardDto> GetOwnerDashboardDataAsync(int ownerId)
        {
            var defects = await _context.Defects
                .Where(d => d.CreatedByUserId == ownerId && !d.IsDeleted)
                .ToListAsync();

            return new OwnerDashboardDto
            {
                TotalDefects = defects.Count,
                CountByStatus = defects.GroupBy(d => d.Status.ToString())
                                       .ToDictionary(g => g.Key, g => g.Count()),
                CountByPriority = defects.GroupBy(d => d.Priority.ToString())
                                         .ToDictionary(g => g.Key, g => g.Count()),
                RecentDefects = defects.OrderByDescending(d => d.CreatedAt)
                                       .Take(5)
                                       .Select(d => new DefectListDto
                                       {
                                           Id = d.Id,
                                           Title = d.Title,
                                           Status = d.Status,
                                           Priority = d.Priority,
                                           CreatedAt = d.CreatedAt,
                                           DueDate = d.DueDate
                                       }).ToList()
            };
        }
        public async Task<List<Defect>> GetDefectsByDeveloperCompanyAsync(int companyId)
        {
            return await _context.Defects
                .Include(d => d.Premise.ConstructionObject)
                .Include(d => d.Comments)
                .Include(d => d.MediaFiles)
                .Where(d => !d.IsDeleted && d.Premise.ConstructionObject.DeveloperCompanyId == companyId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<Defect> GetDefectByIdForEngineerAsync(int defectId, int companyId)
        {
            return await _context.Defects
                .Include(d => d.Premise.ConstructionObject)
                .FirstOrDefaultAsync(d => d.Id == defectId && !d.IsDeleted && d.Premise.ConstructionObject.DeveloperCompanyId == companyId);
        }

        public async Task AssignDefectAsync(int defectId, int? contractorCompanyId, int? assignedUserId, int changedByUserId)
        {
            var defect = await _context.Defects.FindAsync(defectId);
            if (defect == null) return;

            defect.ContractorCompanyId = contractorCompanyId;
            defect.AssignedToUserId = assignedUserId;
            defect.Status = DefectStatus.Assigned;
            defect.UpdatedAt = DateTime.Now;

            var history = new DefectStatusHistory
            {
                DefectId = defectId,
                OldStatus = DefectStatus.Created,
                NewStatus = DefectStatus.Assigned,
                ChangedByUserId = changedByUserId,
                ChangedAt = DateTime.Now,
                Comment = "Дефект назначен подрядчику"
            };
            _context.DefectStatusHistory.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Company>> GetContractorCompaniesAsync()
        {
            return await _context.Companies
                .Where(c => c.CompanyType == CompanyType.Contractor && c.IsActive)
                .ToListAsync();
        }

        public async Task<List<User>> GetUsersByRoleAndCompanyAsync(UserRole role, int? companyId)
        {
            var query = _context.Users.Where(u => u.Role == role && u.IsActive);
            if (companyId.HasValue)
                query = query.Where(u => u.CompanyId == companyId.Value);
            return await query.ToListAsync();
        }

        public async Task UpdateDefectStatusAsync(int defectId, DefectStatus newStatus, int changedByUserId, string comment = null)
        {
            var defect = await _context.Defects.FindAsync(defectId);
            if (defect == null) return;

            var oldStatus = defect.Status;
            defect.Status = newStatus;
            defect.UpdatedAt = DateTime.Now;

            if (newStatus == DefectStatus.Confirmed)
            {
                defect.AcceptedByOwnerAt = DateTime.Now;
                defect.ClosedAt = DateTime.Now;
            }

            var history = new DefectStatusHistory
            {
                DefectId = defectId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedByUserId = changedByUserId,
                ChangedAt = DateTime.Now,
                Comment = comment ?? $"Статус изменён с {oldStatus} на {newStatus}"
            };
            _context.DefectStatusHistory.Add(history);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Defect>> GetDefectsByAssignedUserAsync(int userId)
        {
            return await _context.Defects
                .Include(d => d.Premise.ConstructionObject)
                .Include(d => d.CreatedByUser)
                .Include(d => d.AssignedToUser)
                .Include(d => d.ContractorCompany)
                .Include(d => d.Comments)
                .Include(d => d.MediaFiles)
                .Where(d => !d.IsDeleted && d.AssignedToUserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }
        public async Task<List<Defect>> GetAllDefectsAsync()
        {
            return await _context.Defects
                .Include(d => d.Premise.ConstructionObject.DeveloperCompany)
                .Include(d => d.CreatedByUser)
                .Include(d => d.AssignedToUser)
                .Include(d => d.ContractorCompany)
                .Include(d => d.Comments)
                .Include(d => d.MediaFiles)
                .Where(d => !d.IsDeleted)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

    }
}