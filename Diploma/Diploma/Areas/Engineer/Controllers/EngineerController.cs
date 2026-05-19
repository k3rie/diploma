using Diploma.Controllers;
using Diploma.Helpers;
using Diploma.Models;
using Diploma.Models.DTOs;
using Diploma.Services;
using Diploma.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Diploma.Areas.Engineer.Controllers
{
    [AuthorizeRole(UserRole.DeveloperEngineer)]
    public class EngineerController : BaseController
    {
        private readonly IDefectService _defectService;
        private readonly INotificationService _notificationService;

        public EngineerController()
        {
            _defectService = DependencyResolver.Current.GetService<IDefectService>();
            _notificationService = DependencyResolver.Current.GetService<INotificationService>();
        }

        private int GetCurrentCompanyId()
        {
            var user = UserIdentityHelper.GetCurrentUser();
            return user?.CompanyId ?? throw new UnauthorizedAccessException("Company not assigned");
        }

        public async Task<ActionResult> Index()
        {
            var companyId = GetCurrentCompanyId();
            var dashboard = await _defectService.GetEngineerDashboardAsync(companyId);
            return View(dashboard);
        }

        [HttpGet]
        public async Task<ActionResult> Defects()
        {
            var companyId = GetCurrentCompanyId();
            var defects = await _defectService.GetEngineerDefectsAsync(companyId);
            return View(defects);
        }

        [HttpGet]
        public async Task<ActionResult> DefectDetails(int id)
        {
            var companyId = GetCurrentCompanyId();
            var defect = await _defectService.GetEngineerDefectDetailsAsync(id, companyId);
            if (defect == null)
                return HttpNotFound("Defect not found or access denied");
            ViewBag.Defect = defect;
            return View(defect);
        }

        [HttpGet]
        public async Task<ActionResult> Assign(int id)
        {
            var companyId = GetCurrentCompanyId();
            var defect = await _defectService.GetEngineerDefectDetailsAsync(id, companyId);
            if (defect == null || defect.Status != DefectStatus.Created)
                return RedirectToAction("DefectDetails", new { id });

            var contractors = await _defectService.GetContractorCompaniesAsync();
            ViewBag.Contractors = new SelectList(contractors, "Id", "Name");

            // Список всех подрядчиков (Contractor) для начального наполнения
            var allContractorUsers = await _defectService.GetUsersByRoleAndCompanyAsync(UserRole.Contractor, null);
            ViewBag.ContractorUsers = new SelectList(allContractorUsers, "Id", "FullName");

            // JSON для JavaScript (без использования JsonConvert в представлении)
            ViewBag.InitialUsersJson = new System.Web.Script.Serialization.JavaScriptSerializer()
                .Serialize(allContractorUsers.Select(u => new { Value = u.Id.ToString(), Text = u.FullName }));

            return View(new AssignDefectDto { DefectId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Assign(AssignDefectDto model)
        {
            var user = UserIdentityHelper.GetCurrentUser();
            var companyId = GetCurrentCompanyId();

            // Если выбран флажок «Назначить на себя»
            if (model.AssignToMe)
            {
                model.ContractorCompanyId = null;
                model.AssignedToUserId = user.UserId;
            }

            if (!ModelState.IsValid)
            {
                var contractors = await _defectService.GetContractorCompaniesAsync();
                ViewBag.Contractors = new SelectList(contractors, "Id", "Name", model.ContractorCompanyId);
                ViewBag.ContractorUsers = new SelectList(
                    await _defectService.GetUsersByRoleAndCompanyAsync(UserRole.Contractor, model.ContractorCompanyId),
                    "Id", "FullName", model.AssignedToUserId);
                return View(model);
            }

            try
            {
                await _defectService.AssignDefectAsync(
                    model.DefectId, model.ContractorCompanyId, model.AssignedToUserId, user.UserId, companyId);
                TempData["SuccessMessage"] = "Defect assigned successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }

            return RedirectToAction("DefectDetails", new { id = model.DefectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeStatus(int defectId, DefectStatus newStatus)
        {
            var user = UserIdentityHelper.GetCurrentUser();
            var companyId = GetCurrentCompanyId();
            try
            {
                await _defectService.UpdateDefectStatusAsync(defectId, newStatus, user.UserId);
                TempData["SuccessMessage"] = "Status updated.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("DefectDetails", new { id = defectId });
        }
        [HttpGet]
        public async Task<JsonResult> GetUsersByCompany(int companyId)
        {
            var users = await _defectService.GetUsersByRoleAndCompanyAsync(UserRole.Contractor, companyId);
            var result = users.Select(u => new { Id = u.Id, FullName = u.FullName });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public async Task<PartialViewResult> NotificationBadge()
        {
            var user = UserIdentityHelper.GetCurrentUser();
            var count = await _notificationService.GetUnreadCountAsync(user.UserId);
            return PartialView("_NotificationBadge", count);
        }
        [HttpGet]
        public async Task<JsonResult> GetUnreadNotifications()
        {
            var user = UserIdentityHelper.GetCurrentUser();
            var notifications = await _notificationService.GetUnreadNotificationsAsync(user.UserId);
            var result = notifications.Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                CreatedAt = n.CreatedAt.ToString("dd.MM.yyyy HH:mm")
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}