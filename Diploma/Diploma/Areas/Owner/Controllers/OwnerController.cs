using Diploma.Controllers;
using Diploma.Helpers;
using Diploma.Models;
using Diploma.Models.DTOs;
using Diploma.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Diploma.Areas.Owner.Controllers
{
    [AuthorizeRole(UserRole.Owner)]
    public class OwnerController : BaseController
    {
        private readonly IDefectService _defectService;

        public OwnerController()
        {
            _defectService = DependencyResolver.Current.GetService<IDefectService>();
        }

        // GET: Owner/Owner
        public ActionResult Index()
        {
            return View();
        }

        #region Defects CRUD

        // GET: Owner/Owner/Defects
        [HttpGet]
        public async Task<ActionResult> Defects()
        {
            var currentUser = UserIdentityHelper.GetCurrentUser();
            var defects = await _defectService.GetOwnerDefectsAsync(currentUser.UserId);
            return View(defects);
        }

        // GET: Owner/Owner/DefectDetails/5
        [HttpGet]
        public async Task<ActionResult> DefectDetails(int id)
        {
            var currentUser = UserIdentityHelper.GetCurrentUser();
            var defect = await _defectService.GetDefectDetailsAsync(id, currentUser.UserId);

            if (defect == null)
                return HttpNotFound("Defect not found or you don't have access");

            return View(defect);
        }

        // GET: Owner/Owner/CreateDefect
        [HttpGet]
        public async Task<ActionResult> CreateDefect(int? premiseId)
        {
            var currentUser = UserIdentityHelper.GetCurrentUser();
            var premises = await _defectService.GetOwnerPremisesAsync(currentUser.UserId);

            if (premises!=null)
            {
                TempData["WarningMessage"] = "You don't have any premises assigned. Please contact administrator.";
                return RedirectToAction("Defects");
            }

            ViewBag.Premises = new SelectList(premises, "Id", "Number");

            var model = new DefectCreateDto();
            if (premiseId.HasValue)
            {
                model.PremisesId = premiseId.Value;
            }

            return View(model);
        }

        // POST: Owner/Owner/CreateDefect
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateDefect(DefectCreateDto model)
        {
            var currentUser = UserIdentityHelper.GetCurrentUser();

            if (!ModelState.IsValid)
            {
                var premises = await _defectService.GetOwnerPremisesAsync(currentUser.UserId);
                ViewBag.Premises = new SelectList(premises, "Id", "Number", model.PremisesId);
                return View(model);
            }

            try
            {
                var defect = await _defectService.CreateDefectAsync(model, currentUser.UserId);
                TempData["SuccessMessage"] = "Defect created successfully!";
                return RedirectToAction("DefectDetails", new { id = defect.Id });
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "You don't have access to this premise";
                return RedirectToAction("Defects");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating defect: " + ex.Message);
                var premises = await _defectService.GetOwnerPremisesAsync(currentUser.UserId);
                ViewBag.Premises = new SelectList(premises, "Id", "Number", model.PremisesId);
                return View(model);
            }
        }

        // GET: Owner/Owner/EditDefect/5
        [HttpGet]
        public async Task<ActionResult> EditDefect(int id)
        {
            var currentUser = UserIdentityHelper.GetCurrentUser();
            var hasAccess = await _defectService.HasAccessToDefectAsync(id, currentUser.UserId);

            if (!hasAccess)
                return new HttpStatusCodeResult(403, "You don't have access to this defect");

            var defect = await _defectService.GetDefectDetailsAsync(id, currentUser.UserId);
            if (defect == null)
                return HttpNotFound("Defect not found");

            // Only allow editing if status is "Created"
            if (defect.Status != DefectStatus.Created)
            {
                TempData["WarningMessage"] = "Can only edit defects with 'Created' status";
                return RedirectToAction("DefectDetails", new { id });
            }

            var model = new DefectCreateDto
            {
                Title = defect.Title,
                Description = defect.Description,
                Priority = defect.Priority,
                PremisesId = defect.PremisesId,
                DueDate = defect.DueDate
            };

            var premises = await _defectService.GetOwnerPremisesAsync(currentUser.UserId);
            ViewBag.Premises = new SelectList(premises, "Id", "Number", model.PremisesId);

            return View(model);
        }

        // POST: Owner/Owner/EditDefect/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDefect(int id, DefectCreateDto model)
        {
            var currentUser = UserIdentityHelper.GetCurrentUser();

            if (!ModelState.IsValid)
            {
                var premises = await _defectService.GetOwnerPremisesAsync(currentUser.UserId);
                ViewBag.Premises = new SelectList(premises, "Id", "Number", model.PremisesId);
                return View(model);
            }

            try
            {
                await _defectService.UpdateDefectAsync(id, model, currentUser.UserId);
                TempData["SuccessMessage"] = "Defect updated successfully!";
                return RedirectToAction("DefectDetails", new { id });
            }
            catch (UnauthorizedAccessException)
            {
                return new HttpStatusCodeResult(403);
            }
            catch (KeyNotFoundException)
            {
                return HttpNotFound("Defect not found");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating defect: " + ex.Message);
                var premises = await _defectService.GetOwnerPremisesAsync(currentUser.UserId);
                ViewBag.Premises = new SelectList(premises, "Id", "Number", model.PremisesId);
                return View(model);
            }
        }

        // POST: Owner/Owner/DeleteDefect/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteDefect(int id)
        {
            var currentUser = UserIdentityHelper.GetCurrentUser();

            try
            {
                await _defectService.DeleteDefectAsync(id, currentUser.UserId);
                TempData["SuccessMessage"] = "Defect deleted successfully!";
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "You don't have access to delete this defect";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error deleting defect";
            }

            return RedirectToAction("Defects");
        }

        // POST: Owner/Owner/ConfirmDefectFix/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmDefectFix(int id)
        {
            var currentUser = UserIdentityHelper.GetCurrentUser();

            try
            {
                await _defectService.ConfirmDefectFixAsync(id, currentUser.UserId);
                TempData["SuccessMessage"] = "Defect fix confirmed successfully!";
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "You don't have access to confirm this defect";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error confirming defect fix";
            }

            return RedirectToAction("DefectDetails", new { id });
        }

        // GET: Owner/Owner/MyPremises
        [HttpGet]
        public async Task<ActionResult> MyPremises()
        {
            var currentUser = UserIdentityHelper.GetCurrentUser();
            var premises = await _defectService.GetOwnerPremisesAsync(currentUser.UserId);
            return View(premises);
        }

        #endregion
    }
}