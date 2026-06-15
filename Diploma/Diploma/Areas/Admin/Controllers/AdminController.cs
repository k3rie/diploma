using Diploma.Controllers;
using Diploma.Helpers;
using Diploma.Models;
using Diploma.Models.DTOs;
using Diploma.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Diploma.Areas.Admin.Controllers
{
    [AuthorizeRole(UserRole.Admin)]
    public class AdminController : BaseController
    {
        private readonly IDefectService _defectService;
        private readonly IUserService _userService;
        private readonly ICompanyService _companyService;
        private readonly IObjectService _objectService;

        public AdminController()
        {
            _defectService = DependencyResolver.Current.GetService<IDefectService>();
            _userService = DependencyResolver.Current.GetService<IUserService>();
            _companyService = DependencyResolver.Current.GetService<ICompanyService>();
            _objectService = DependencyResolver.Current.GetService<IObjectService>();
        }

        // ==================== Дашборд и дефекты ====================
        public async Task<ActionResult> Index()
        {
            var dashboard = await _defectService.GetAdminDashboardAsync();
            var defects = await _defectService.GetAllDefectsAsync();
            ViewBag.Companies = new SelectList(await _companyService.GetAllCompaniesAsync(), "Id", "Name");
            ViewBag.Objects = new SelectList(await _objectService.GetAllObjectsAsync(), "Id", "Name");
            ViewBag.Statuses = new SelectList(
                Enum.GetValues(typeof(DefectStatus)).Cast<DefectStatus>()
                    .Select(s => new { Value = (int)s, Text = s.GetDisplayName() }),
                "Value", "Text");

            ViewBag.CurrentCompany = null;
            ViewBag.CurrentObject = null;
            ViewBag.CurrentStatus = null;

            var model = new AdminIndexViewModel
            {
                Dashboard = dashboard,
                Defects = defects
            };
            return View(model);
        }

       
        [HttpGet]
        public async Task<ActionResult> FilterDefectsAndStats(int? companyId, int? objectId, int? status)
        {
            var dashboard = await _defectService.GetAdminDashboardAsync(companyId, objectId, status);
            var defects = await _defectService.GetFilteredDefectsAsync(companyId, objectId, null,
                status.HasValue ? (DefectStatus)status.Value : (DefectStatus?)null);

            ViewBag.Companies = new SelectList(await _companyService.GetAllCompaniesAsync(), "Id", "Name");
            ViewBag.Objects = new SelectList(await _objectService.GetAllObjectsAsync(), "Id", "Name");
            ViewBag.Statuses = new SelectList(
                Enum.GetValues(typeof(DefectStatus)).Cast<DefectStatus>()
                    .Select(s => new { Value = (int)s, Text = s.GetDisplayName() }),
                "Value", "Text");

            ViewBag.CurrentCompany = companyId;
            ViewBag.CurrentObject = objectId;
            ViewBag.CurrentStatus = status;

            var model = new AdminIndexViewModel { Dashboard = dashboard, Defects = defects };
            return PartialView("_DashboardContent", model);
        }

        // ==================== Пользователи ====================
        public async Task<ActionResult> Users()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<ActionResult> CreateUser()
        {
            ViewBag.Companies = new SelectList(await _companyService.GetAllCompaniesAsync(), "Id", "Name");
            ViewBag.Roles = GetRolesSelectList();
            return PartialView("_CreateUser", new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateUser(User model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Companies = new SelectList(await _companyService.GetAllCompaniesAsync(), "Id", "Name", model.CompanyId);
                ViewBag.Roles = GetRolesSelectList((int)model.Role);
                return PartialView("_CreateUser", model);
            }
            if (!string.IsNullOrWhiteSpace(model.PasswordHash))
                model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
            await _userService.CreateUserAsync(model);
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteUser(int id)
        {
            await _userService.DeleteUserAsync(id);
            TempData["SuccessMessage"] = "Пользователь удалён.";
            return RedirectToAction("Users");
        }

        public async Task<ActionResult> UserDetails(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        [HttpGet]
        public async Task<ActionResult> EditUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return HttpNotFound();
            ViewBag.Companies = new SelectList(await _companyService.GetAllCompaniesAsync(), "Id", "Name", user.CompanyId);
            ViewBag.Roles = GetRolesSelectList((int)user.Role);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUser(User model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Companies = new SelectList(await _companyService.GetAllCompaniesAsync(), "Id", "Name", model.CompanyId);
                ViewBag.Roles = GetRolesSelectList((int)model.Role);
                return View(model);
            }
            var existingUser = await _userService.GetUserByIdAsync(model.Id);
            if (!string.IsNullOrWhiteSpace(model.PasswordHash))
                model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
            else
                model.PasswordHash = existingUser.PasswordHash;
            model.CreatedAt = existingUser.CreatedAt; // сохраняем дату создания
            await _userService.UpdateUserAsync(model);
            TempData["SuccessMessage"] = "Пользователь обновлён.";
            return RedirectToAction("UserDetails", new { id = model.Id });
        }

        // ==================== Компании ====================
        public async Task<ActionResult> Companies()
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return View(companies);
        }

        [HttpGet]
        public async Task<ActionResult> CreateCompany()
        {
            ViewBag.CompanyTypes = GetCompanyTypesSelectList();
            return PartialView("_CreateCompany", new Company());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCompany(Company model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CompanyTypes = GetCompanyTypesSelectList((int)model.CompanyType);
                return PartialView("_CreateCompany", model);
            }
            await _companyService.CreateCompanyAsync(model);
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteCompany(int id)
        {
            await _companyService.DeleteCompanyAsync(id);
            TempData["SuccessMessage"] = "Компания удалена.";
            return RedirectToAction("Companies");
        }

        public async Task<ActionResult> CompanyDetails(int id)
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null) return HttpNotFound();
            return View(company);
        }

        [HttpGet]
        public async Task<ActionResult> EditCompany(int id)
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null) return HttpNotFound();
            ViewBag.CompanyTypes = GetCompanyTypesSelectList((int)company.CompanyType);
            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCompany(Company model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CompanyTypes = GetCompanyTypesSelectList((int)model.CompanyType);
                return View(model);
            }
            await _companyService.UpdateCompanyAsync(model);
            TempData["SuccessMessage"] = "Компания обновлена.";
            return RedirectToAction("CompanyDetails", new { id = model.Id });
        }

        // ==================== Объекты ====================
        public async Task<ActionResult> Objects()
        {
            var objects = await _objectService.GetAllObjectsAsync();
            return View(objects);
        }

        [HttpGet]
        public async Task<ActionResult> CreateObject()
        {
            ViewBag.DeveloperCompanies = new SelectList(await _companyService.GetAllCompaniesAsync(), "Id", "Name");
            return PartialView("_CreateObject", new ConstructionObject());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateObject(ConstructionObject model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.DeveloperCompanies = new SelectList(await _companyService.GetAllCompaniesAsync(), "Id", "Name", model.DeveloperCompanyId);
                return PartialView("_CreateObject", model);
            }
            await _objectService.CreateObjectAsync(model);
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteObject(int id)
        {
            await _objectService.DeleteObjectAsync(id);
            TempData["SuccessMessage"] = "Объект удалён.";
            return RedirectToAction("Objects");
        }

        public async Task<ActionResult> ObjectDetails(int id)
        {
            var obj = await _objectService.GetObjectByIdAsync(id);
            if (obj == null) return HttpNotFound();
            var premises = await _objectService.GetPremisesByObjectAsync(id);
            ViewBag.Premises = premises;
            return View(obj);
        }

        [HttpGet]
        public async Task<ActionResult> EditObject(int id)
        {
            var obj = await _objectService.GetObjectByIdAsync(id);
            if (obj == null) return HttpNotFound();
            ViewBag.DeveloperCompanies = new SelectList(await _companyService.GetAllCompaniesAsync(), "Id", "Name", obj.DeveloperCompanyId);
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditObject(ConstructionObject model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.DeveloperCompanies = new SelectList(await _companyService.GetAllCompaniesAsync(), "Id", "Name", model.DeveloperCompanyId);
                return View(model);
            }
            await _objectService.UpdateObjectAsync(model);
            TempData["SuccessMessage"] = "Объект обновлён.";
            return RedirectToAction("ObjectDetails", new { id = model.Id });
        }

        // ==================== Помещения ====================
        [HttpGet]
        public async Task<ActionResult> CreatePremise(int objectId)
        {
            ViewBag.ObjectId = objectId;
            ViewBag.Users = new SelectList(await _userService.GetAllUsersAsync(), "Id", "FullName");
            return PartialView("_CreatePremise", new Premise { ObjectId = objectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatePremise(Premise model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Users = new SelectList(await _userService.GetAllUsersAsync(), "Id", "FullName", model.OwnerId);
                return PartialView("_CreatePremise", model);
            }
            await _objectService.CreatePremiseAsync(model);
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<ActionResult> EditPremise(int id)
        {
            var premise = await _objectService.GetPremiseByIdAsync(id);
            if (premise == null) return HttpNotFound();
            ViewBag.Users = new SelectList(await _userService.GetAllUsersAsync(), "Id", "FullName", premise.OwnerId);
            return View(premise);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPremise(Premise model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Users = new SelectList(await _userService.GetAllUsersAsync(), "Id", "FullName", model.OwnerId);
                return View(model);
            }
            await _objectService.UpdatePremiseAsync(model);
            TempData["SuccessMessage"] = "Помещение обновлено.";
            return RedirectToAction("ObjectDetails", new { id = model.ObjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePremise(int id, int objectId)
        {
            await _objectService.DeletePremiseAsync(id);
            TempData["SuccessMessage"] = "Помещение удалено.";
            return RedirectToAction("ObjectDetails", new { id = objectId });
        }

        // ==================== Вспомогательные методы ====================
        private SelectList GetRolesSelectList(int? selected = null)
        {
            var roles = new[]
            {
                new { Value = (int)UserRole.Owner, Text = "Собственник" },
                new { Value = (int)UserRole.DeveloperEngineer, Text = "Инженер" },
                new { Value = (int)UserRole.Contractor, Text = "Подрядчик" },
                new { Value = (int)UserRole.Admin, Text = "Администратор" }
            };
            return new SelectList(roles, "Value", "Text", selected);
        }

        private SelectList GetCompanyTypesSelectList(int? selected = null)
        {
            var types = new[]
            {
                new { Value = (int)CompanyType.Developer, Text = "Застройщик" },
                new { Value = (int)CompanyType.Contractor, Text = "Подрядчик" },
                new { Value = (int)CompanyType.ManagementCompany, Text = "Управляющая компания" }
            };
            return new SelectList(types, "Value", "Text", selected);
        }
    }
}