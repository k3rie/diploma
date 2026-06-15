using Diploma.Controllers;
using Diploma.Helpers;
using Diploma.Models;
using Diploma.Models.DTOs;
using Diploma.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Diploma.Areas.Contractor.Controllers
{
    [AuthorizeRole(UserRole.Contractor)]
    public class ContractorController : BaseController
    {
        private readonly IDefectService _defectService;
        private readonly INotificationService _notificationService;

        public ContractorController()
        {
            _defectService = DependencyResolver.Current.GetService<IDefectService>();
            _notificationService = DependencyResolver.Current.GetService<INotificationService>();
        }

        private int GetCurrentUserId()
        {
            var user = UserIdentityHelper.GetCurrentUser();
            return user?.UserId ?? throw new UnauthorizedAccessException("Not authorized");
        }

        public async Task<ActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var defects = await _defectService.GetContractorDefectsAsync(userId);

            var model = new ContractorDashboardDto
            {
                TotalDefects = defects.Count,
                CountByStatus = defects
                    .GroupBy(d => d.Status.GetDisplayName()) // вернёт русское название статуса
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Defects()
        {
            var userId = GetCurrentUserId();
            var defects = await _defectService.GetContractorDefectsAsync(userId);
            return View(defects);
        }

        [HttpGet]
        public async Task<ActionResult> DefectDetails(int id)
        {
            var userId = GetCurrentUserId();
            var defect = await _defectService.GetDefectDetailForContractorAsync(id, userId);
            if (defect == null)
                return HttpNotFound();
            ViewBag.Defect = defect;
            return View(defect);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeStatus(int defectId, DefectStatus newStatus)
        {
            var userId = GetCurrentUserId();
            var defect = await _defectService.GetDefectDetailForContractorAsync(defectId, userId);
            if (defect == null)
                return HttpNotFound();
            await _defectService.UpdateDefectStatusAsync(defectId, newStatus, userId);
            TempData["SuccessMessage"] = "Status updated.";
            return RedirectToAction("DefectDetails", new { id = defectId });
        }

        [HttpGet]
        public ActionResult MarkFixed(int defectId)
        {
            return PartialView("_MarkFixed", new MarkFixedDto { DefectId = defectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkFixed(MarkFixedDto model)
        {
            var userId = GetCurrentUserId();
            try
            {
                await _defectService.MarkDefectFixedWithPhotosAsync(model.DefectId, userId, model.Photos);
                TempData["SuccessMessage"] = "Defect fixed.";
            }
            catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; }
            return RedirectToAction("DefectDetails", new { id = model.DefectId });
        }

        [HttpGet]
        public async Task<JsonResult> GetUnreadNotifications()
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
            var result = notifications.Select(n => new { n.Id, n.Title, n.Message, CreatedAt = n.CreatedAt.ToString("dd.MM.yyyy HH:mm") });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddComment(int defectId, string message)
        {
            var user = UserIdentityHelper.GetCurrentUser();
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] = "Комментарий не может быть пустым.";
                return RedirectToAction("DefectDetails", new { id = defectId });
            }
            try
            {
                await _defectService.AddCommentAsync(defectId, user.UserId, message);
                TempData["SuccessMessage"] = "Комментарий добавлен.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("DefectDetails", new { id = defectId });
        }
    }
}