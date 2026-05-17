using Diploma.Data.Interfaces;
using Diploma.Models;
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
                Comment = "Defect created"
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
    }
}