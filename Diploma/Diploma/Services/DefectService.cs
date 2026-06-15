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
using System.Web.Mvc;

namespace Diploma.Services
{
    public class DefectService : IDefectService
    {
        private readonly IDefectRepository _defectRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository; // добавить

        public DefectService(IDefectRepository defectRepository, INotificationService notificationService, IUserRepository userRepository)
        {
            _defectRepository = defectRepository;
            _notificationService = notificationService;
            _userRepository = userRepository;
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
            var defectnot = await _defectRepository.GetByIdWithDetailsAsync(defectId);
            if (defectnot != null)
            {
                // Уведомить инженера, если он не владелец
                if (defectnot.AssignedToUserId.HasValue && defectnot.AssignedToUserId != userId)
                    await _notificationService.NotifyAsync(defectnot.AssignedToUserId.Value, "Дефект подтверждён", $"Дефект '{defectnot.Title}' был подтверждён владельцем.");
                // Уведомить застройщика (всех инженеров компании) – опционально
                var companyId = defectnot.Premise?.ConstructionObject?.DeveloperCompanyId;
                if (companyId.HasValue)
                {
                    var engineers = await _defectRepository.GetUsersByRoleAndCompanyAsync(UserRole.DeveloperEngineer, companyId);
                    foreach (var eng in engineers.Where(e => e.Id != userId))
                        await _notificationService.NotifyAsync(eng.Id, "Дефект подтверждён", $"Дефект '{defectnot.Title}' подтверждён владельцем.");
                }
            }

            await _defectRepository.UpdateStatusAsync(defectId, DefectStatus.Confirmed, userId, "Исправление подтверждено владельцем");
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
        public async Task<OwnerDashboardDto> GetOwnerDashboardAsync(int ownerId)
        {
            return await _defectRepository.GetOwnerDashboardDataAsync(ownerId);
        }

        public async Task<EngineerDashboardDto> GetEngineerDashboardAsync(int companyId)
        {
            var defects = await _defectRepository.GetDefectsByDeveloperCompanyAsync(companyId);
            return new EngineerDashboardDto
            {
                TotalDefects = defects.Count,
                CountByStatus = defects.GroupBy(d => d.Status.GetDisplayName())
                                       .ToDictionary(g => g.Key, g => g.Count()),
                CountByPriority = defects.GroupBy(d => d.Priority.GetDisplayName())
                                         .ToDictionary(g => g.Key, g => g.Count()),
                OverdueCount = defects.Count(d => d.DueDate.HasValue && d.DueDate < DateTime.Now && d.Status != DefectStatus.Confirmed),
                AverageFixTime = defects.Where(d => d.ClosedAt.HasValue)
                                        .Average(d => (d.ClosedAt.Value - d.CreatedAt).TotalDays)
            };
        }

        public async Task<List<DefectListDto>> GetEngineerDefectsAsync(int companyId)
        {
            var defects = await _defectRepository.GetDefectsByDeveloperCompanyAsync(companyId);
            return defects.Select(MapToListDto).ToList();
        }

        public async Task<DefectDetailDto> GetEngineerDefectDetailsAsync(int defectId, int companyId)
        {
            var defect = await _defectRepository.GetByIdWithDetailsAsync(defectId);
            if (defect == null || defect.Premise.ConstructionObject.DeveloperCompanyId != companyId)
                return null;
            return MapToDetailDto(defect);
        }

        public async Task AssignDefectAsync(int defectId, int? contractorCompanyId, int? assignedUserId, int changedByUserId, int companyId)
        {
            // Проверяем, что дефект принадлежит компании инженера
            var defect = await _defectRepository.GetDefectByIdForEngineerAsync(defectId, companyId);
            if (defect == null)
                throw new KeyNotFoundException("Defect not found or access denied");
            // Уведомить владельца
            if (defect.CreatedByUserId != changedByUserId)
                await _notificationService.NotifyAsync(defect.CreatedByUserId, "Дефект назначен", $"Ваш дефект '{defect.Title}' был назначен.");

            // Уведомить подрядчика, если не сам себя назначил
            if (assignedUserId.HasValue && assignedUserId.Value != changedByUserId)
                await _notificationService.NotifyAsync(assignedUserId.Value, "Назначен новый дефект", $"Вы назначены на дефект '{defect.Title}'.");
            await _defectRepository.AssignDefectAsync(defectId, contractorCompanyId, assignedUserId, changedByUserId);
        }

        // Обновим сигнатуру AssignDefectAsync в интерфейсе и сервисе, добавив companyId, но можем обойтись без него, если получим через changedByUserId.
        // Сделаем: в сервисе AssignDefectAsync(int defectId, int? contractorCompanyId, int? assignedUserId, int changedByUserId) будет проверять, что changedByUserId имеет роль Engineer и принадлежит компании-застройщику, к которой относится дефект.
        // Но для упрощения сейчас оставим как есть, а проверку доступа сделаем в контроллере (получим companyId текущего пользователя и проверим через GetDefectByIdForEngineerAsync).

        public async Task UpdateDefectStatusAsync(int defectId, DefectStatus newStatus, int changedByUserId)
        {
            var defect = await _defectRepository.GetByIdWithDetailsAsync(defectId);
            await _defectRepository.UpdateDefectStatusAsync(defectId, newStatus, changedByUserId);

            if (defect != null)
            {
                string title = $"Статус изменён на {newStatus}";
                string message = $"Статус дефекта '{defect.Title}' в помещении {defect.Premise?.Number} изменён на {newStatus}.";

                if (defect.CreatedByUserId != changedByUserId)
                    await _notificationService.NotifyAsync(defect.CreatedByUserId, title, message);

                if (defect.AssignedToUserId.HasValue && defect.AssignedToUserId != changedByUserId)
                    await _notificationService.NotifyAsync(defect.AssignedToUserId.Value, title, message);
            }
        }

        public async Task<List<Company>> GetContractorCompaniesAsync()
        {
            return await _defectRepository.GetContractorCompaniesAsync();
        }

        public async Task<List<User>> GetUsersByRoleAndCompanyAsync(UserRole role, int? companyId)
        {
            return await _defectRepository.GetUsersByRoleAndCompanyAsync(role, companyId);
        }
        public async Task MarkDefectFixedWithPhotosAsync(int defectId, int userId, IEnumerable<HttpPostedFileBase> photos)
        {
            var defect = await _defectRepository.GetByIdAsync(defectId);
            if (defect == null) throw new KeyNotFoundException("Defect not found");
            if (defect.Status != DefectStatus.InProgress)
                throw new InvalidOperationException("Only defects in progress can be marked fixed");

            // Определяем роль пользователя
            var user = await _userRepository.GetByIdAsync(userId);
            string comment = user?.Role == UserRole.Contractor ? "Исправлен подрядчиком" : "Исправлен инженером";

            // Смена статуса с кастомным комментарием
            await _defectRepository.UpdateDefectStatusAsync(defectId, DefectStatus.Fixed, userId, comment);

            // Сохранение фото после исправления
            if (photos != null)
            {
                foreach (var file in photos)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        var media = await SaveDefectMedia(file, defectId, userId, isBeforeFix: false);
                        await _defectRepository.AddMediaAsync(media);
                    }
                }
            }

            // Уведомления
            var defectFull = await _defectRepository.GetByIdWithDetailsAsync(defectId);
            if (defectFull != null)
            {
                await _notificationService.NotifyAsync(defectFull.CreatedByUserId, "Дефект исправлен", $"Дефект '{defectFull.Title}' отмечен как исправленный.");
                if (defectFull.AssignedToUserId.HasValue && defectFull.AssignedToUserId != userId)
                    await _notificationService.NotifyAsync(defectFull.AssignedToUserId.Value, "Дефект исправлен", $"Дефект '{defectFull.Title}' отмечен как исправленный.");
            }
        }

        // Вспомогательный метод для сохранения с параметром isBeforeFix
        private async Task<DefectMedia> SaveDefectMedia(HttpPostedFileBase file, int defectId, int userId, bool isBeforeFix = true)
        {
            var uploadsFolder = System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads/Defects/");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            file.SaveAs(Path.Combine(uploadsFolder, uniqueFileName));
            return new DefectMedia
            {
                DefectId = defectId,
                OriginalFileName = file.FileName,
                StoredFileName = uniqueFileName,
                ContentType = file.ContentType,
                FileSize = file.ContentLength,
                UploadedByUserId = userId,
                IsBeforeFix = isBeforeFix,
                UploadedAt = DateTime.Now
            };
        }
        public async Task<List<DefectListDto>> GetContractorDefectsAsync(int userId)
        {
            var defects = await _defectRepository.GetDefectsByAssignedUserAsync(userId);
            return defects.Select(MapToListDto).ToList();
        }
        public async Task<DefectDetailDto> GetDefectDetailForContractorAsync(int defectId, int userId)
        {
            var defect = await _defectRepository.GetByIdWithDetailsAsync(defectId);
            if (defect == null || defect.AssignedToUserId != userId)
                return null;
            return MapToDetailDto(defect);
        }
        public async Task AddCommentAsync(int defectId, int userId, string message)
        {
            var defect = await _defectRepository.GetByIdAsync(defectId);
            if (defect == null) throw new KeyNotFoundException("Дефект не найден");

            var comment = new DefectComment
            {
                DefectId = defectId,
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.Now
            };
            await _defectRepository.AddCommentAsync(comment);
        }
        public async Task RejectFixAsync(int defectId, int userId, string comment)
        {
            var hasAccess = await _defectRepository.HasAccessToDefectAsync(defectId, userId);
            if (!hasAccess)
                throw new UnauthorizedAccessException("Нет доступа к дефекту");

            var defect = await _defectRepository.GetByIdAsync(defectId);
            if (defect == null) throw new KeyNotFoundException("Дефект не найден");
            if (defect.Status != DefectStatus.Fixed)
                throw new InvalidOperationException("Можно отклонить только исправленный дефект");

            await _defectRepository.UpdateDefectStatusAsync(defectId, DefectStatus.InProgress, userId,
                string.IsNullOrWhiteSpace(comment) ? "Исправление отклонено владельцем" : comment);

            var defectFull = await _defectRepository.GetByIdWithDetailsAsync(defectId);
            if (defectFull?.AssignedToUserId != null && defectFull.AssignedToUserId != userId)
            {
                await _notificationService.NotifyAsync(defectFull.AssignedToUserId.Value,
                    "Исправление отклонено",
                    $"Исправление дефекта '{defectFull.Title}' отклонено владельцем. Причина: {comment}");
            }
        }
        // DefectService
        public async Task<List<DefectListDto>> GetAllDefectsAsync()
        {
            var defects = await _defectRepository.GetAllDefectsAsync();
            return defects.Select(MapToListDto).ToList();
        }

        public async Task<List<DefectListDto>> GetFilteredDefectsAsync(int? companyId = null, int? objectId = null, int? premiseId = null, DefectStatus? status = null)
        {
            var defects = await _defectRepository.GetAllDefectsAsync();
            if (companyId.HasValue)
                defects = defects.Where(d => d.Premise.ConstructionObject.DeveloperCompanyId == companyId.Value).ToList();
            if (objectId.HasValue)
                defects = defects.Where(d => d.Premise.ObjectId == objectId.Value).ToList();
            if (premiseId.HasValue)
                defects = defects.Where(d => d.PremisesId == premiseId.Value).ToList();
            if (status.HasValue)
                defects = defects.Where(d => d.Status == status.Value).ToList();
            return defects.Select(MapToListDto).ToList();
        }
        public async Task<AdminDashboardDto> GetAdminDashboardAsync(int? companyId = null, int? objectId = null, int? status = null)
        {
            var defects = await _defectRepository.GetAllDefectsAsync(); // базовый список

            if (companyId.HasValue)
                defects = defects.Where(d => d.Premise.ConstructionObject.DeveloperCompanyId == companyId.Value).ToList();
            if (objectId.HasValue)
                defects = defects.Where(d => d.Premise.ObjectId == objectId.Value).ToList();
            if (status.HasValue)
                defects = defects.Where(d => d.Status == (DefectStatus)status.Value).ToList();

            return new AdminDashboardDto
            {
                TotalDefects = defects.Count,
                CountByStatus = defects.GroupBy(d => d.Status.GetDisplayName())
                                       .ToDictionary(g => g.Key, g => g.Count()),
                OverdueCount = defects.Count(d => d.DueDate.HasValue && d.DueDate < DateTime.Now && d.Status != DefectStatus.Confirmed),
                AverageFixTime = defects.Where(d => d.ClosedAt.HasValue)
                                        .Average(d => (d.ClosedAt.Value - d.CreatedAt).TotalDays)
            };
        }
        #endregion
    }
}