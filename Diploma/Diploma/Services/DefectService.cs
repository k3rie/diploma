using Diploma.Data.Interfaces;
using Diploma.Helpers;
using Diploma.Models;
using Diploma.Models.DTOs;
using Diploma.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Diploma.Services
{
    public class DefectService : IDefectService
    {
        private readonly IDefectRepository _defectRepository;

        public DefectService(IDefectRepository defectRepository)
        {
            _defectRepository = defectRepository;
        }

        public async Task<DefectDetailDto> GetDefectDetailsAsync(int defectId, int userId)
        {
            var hasAccess = await _defectRepository.HasAccessToDefectAsync(defectId, userId);
            if (!hasAccess)
                return null;

            var defect = await _defectRepository.GetByIdWithDetailsAsync(defectId);
            if (defect == null)
                return null;

            return MapToDetailDto(defect);
        }

        public async Task<List<DefectListDto>> GetOwnerDefectsAsync(int ownerId)
        {
            var defects = await _defectRepository.GetByOwnerIdAsync(ownerId);
            return defects.Select(MapToListDto).ToList();
        }

        public async Task<List<DefectListDto>> GetPremiseDefectsAsync(int premiseId, int ownerId)
        {
            var premise = await _defectRepository.GetPremiseByIdAsync(premiseId);
            if (premise == null || premise.OwnerId != ownerId)
                return new List<DefectListDto>();

            var defects = await _defectRepository.GetByPremiseIdAsync(premiseId);
            return defects.Select(MapToListDto).ToList();
        }

        public async Task<DefectDetailDto> CreateDefectAsync(DefectCreateDto model, int userId)
        {
            var premise = await _defectRepository.GetPremiseByIdAsync(model.PremisesId);
            if (premise == null || premise.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have access to this premise");

            var defect = new Defect
            {
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority,
                Status = DefectStatus.Created,
                PremisesId = model.PremisesId,
                CreatedByUserId = userId,
                DueDate = model.DueDate,
                CreatedAt = DateTime.Now
            };

            var createdDefect = await _defectRepository.CreateAsync(defect);

            // Upload photos if any
            if (model.Photos != null && model.Photos.Any())
            {
                foreach (var photo in model.Photos)
                {
                    if (photo != null && photo.ContentLength > 0)
                    {
                        var media = await SaveDefectMedia(photo, createdDefect.Id, userId);
                        await _defectRepository.AddMediaAsync(media);
                    }
                }
            }

            return await GetDefectDetailsAsync(createdDefect.Id, userId);
        }

        public async Task UpdateDefectAsync(int defectId, DefectCreateDto model, int userId)
        {
            var hasAccess = await _defectRepository.HasAccessToDefectAsync(defectId, userId);
            if (!hasAccess)
                throw new UnauthorizedAccessException("You don't have access to this defect");

            var defect = await _defectRepository.GetByIdAsync(defectId);
            if (defect == null)
                throw new KeyNotFoundException("Defect not found");

            defect.Title = model.Title;
            defect.Description = model.Description;
            defect.Priority = model.Priority;
            defect.DueDate = model.DueDate;

            await _defectRepository.UpdateAsync(defect);

            // Upload new photos if any
            if (model.Photos != null && model.Photos.Any())
            {
                foreach (var photo in model.Photos)
                {
                    if (photo != null && photo.ContentLength > 0)
                    {
                        var media = await SaveDefectMedia(photo, defectId, userId);
                        await _defectRepository.AddMediaAsync(media);
                    }
                }
            }
        }

        public async Task DeleteDefectAsync(int defectId, int userId)
        {
            var hasAccess = await _defectRepository.HasAccessToDefectAsync(defectId, userId);
            if (!hasAccess)
                throw new UnauthorizedAccessException("You don't have access to this defect");

            var defect = await _defectRepository.GetByIdAsync(defectId);
            if (defect == null)
                throw new KeyNotFoundException("Defect not found");

            if (defect.Status != DefectStatus.Created)
                throw new InvalidOperationException("Can only delete defects with 'Created' status");

            await _defectRepository.SoftDeleteAsync(defectId);
        }

        public async Task ConfirmDefectFixAsync(int defectId, int userId)
        {
            var hasAccess = await _defectRepository.HasAccessToDefectAsync(defectId, userId);
            if (!hasAccess)
                throw new UnauthorizedAccessException("You don't have access to this defect");

            var defect = await _defectRepository.GetByIdAsync(defectId);
            if (defect == null)
                throw new KeyNotFoundException("Defect not found");

            if (defect.Status != DefectStatus.Fixed)
                throw new InvalidOperationException("Can only confirm defects with 'Fixed' status");

            await _defectRepository.UpdateStatusAsync(defectId, DefectStatus.Confirmed, userId, "Fix confirmed by owner");
        }

        public async Task<bool> HasAccessToDefectAsync(int defectId, int userId)
        {
            return await _defectRepository.HasAccessToDefectAsync(defectId, userId);
        }

        public async Task<List<PremiseDto>> GetOwnerPremisesAsync(int ownerId)
        {
            var premises = await _defectRepository.GetOwnerPremisesAsync(ownerId);
            return premises.Select(p => new PremiseDto
            {
                Id = p.Id,
                Number = p.Number,
                Floor = p.Floor,
                Area = p.Area,
                ObjectName = p.ConstructionObject?.Name,
                ObjectId = p.ObjectId,
                DefectCount = p.Defects?.Count(d => !d.IsDeleted) ?? 0
            }).ToList();
        }

        public async Task<PremiseDto> GetPremiseDetailsAsync(int premiseId)
        {
            var premise = await _defectRepository.GetPremiseByIdAsync(premiseId);
            if (premise == null) return null;

            return new PremiseDto
            {
                Id = premise.Id,
                Number = premise.Number,
                Floor = premise.Floor,
                Area = premise.Area,
                ObjectName = premise.ConstructionObject?.Name,
                ObjectId = premise.ObjectId
            };
        }

        #region Helper Methods

        private DefectDetailDto MapToDetailDto(Defect defect)
        {
            return new DefectDetailDto
            {
                Id = defect.Id,
                Title = defect.Title,
                Description = defect.Description,
                Status = defect.Status,
                Priority = defect.Priority,
                PremisesId = defect.PremisesId,
                PremiseNumber = defect.Premise?.Number,
                Floor = defect.Premise?.Floor ?? 0,
                ObjectName = defect.Premise?.ConstructionObject?.Name,
                CreatedByUserName = defect.CreatedByUser?.FullName,
                AssignedToUserName = defect.AssignedToUser?.FullName,
                ContractorCompanyName = defect.ContractorCompany?.Name,
                CreatedAt = defect.CreatedAt,
                UpdatedAt = defect.UpdatedAt,
                DueDate = defect.DueDate,
                ClosedAt = defect.ClosedAt,
                AcceptedByOwnerAt = defect.AcceptedByOwnerAt,
                Comments = defect.Comments?.Select(c => new DefectCommentDto
                {
                    Id = c.Id,
                    Message = c.Message,
                    UserName = c.User?.UserName,
                    UserFullName = c.User?.FullName,
                    CreatedAt = c.CreatedAt
                }).OrderBy(c => c.CreatedAt).ToList() ?? new List<DefectCommentDto>(),
                MediaFiles = defect.MediaFiles?.Select(m => new DefectMediaDto
                {
                    Id = m.Id,
                    OriginalFileName = m.OriginalFileName,
                    StoredFileName = m.StoredFileName,
                    ContentType = m.ContentType,
                    IsBeforeFix = m.IsBeforeFix,
                    UploadedAt = m.UploadedAt
                }).OrderBy(m => m.UploadedAt).ToList() ?? new List<DefectMediaDto>(),
                StatusHistory = defect.StatusHistory?.Select(h => new DefectStatusHistoryDto
                {
                    Id = h.Id,
                    OldStatus = h.OldStatus.ToString(),
                    NewStatus = h.NewStatus.ToString(),
                    ChangedByUserName = h.ChangedByUser?.FullName,
                    ChangedAt = h.ChangedAt,
                    Comment = h.Comment
                }).OrderByDescending(h => h.ChangedAt).ToList() ?? new List<DefectStatusHistoryDto>()
            };
        }

        private DefectListDto MapToListDto(Defect defect)
        {
            return new DefectListDto
            {
                Id = defect.Id,
                Title = defect.Title,
                Status = defect.Status,
                Priority = defect.Priority,
                PremiseNumber = defect.Premise?.Number,
                Floor = defect.Premise?.Floor ?? 0,
                ObjectName = defect.Premise?.ConstructionObject?.Name,
                CreatedAt = defect.CreatedAt,
                DueDate = defect.DueDate,
                CommentCount = defect.Comments?.Count ?? 0,
                MediaCount = defect.MediaFiles?.Count ?? 0
            };
        }

        private async Task<DefectMedia> SaveDefectMedia(HttpPostedFileBase file, int defectId, int userId)
        {
            var uploadsFolder = System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads/Defects/");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            file.SaveAs(filePath);

            return new DefectMedia
            {
                DefectId = defectId,
                OriginalFileName = file.FileName,
                StoredFileName = uniqueFileName,
                ContentType = file.ContentType,
                FileSize = file.ContentLength,
                UploadedByUserId = userId,
                IsBeforeFix = true,
                UploadedAt = DateTime.Now
            };
        }

        #endregion
    }
}