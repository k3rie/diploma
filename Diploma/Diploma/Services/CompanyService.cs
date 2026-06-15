using Diploma.Data;
using Diploma.Models;
using Diploma.Services.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Diploma.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly DefectDbContext _context;
        public CompanyService(DefectDbContext context) { _context = context; }

        public async Task<List<Company>> GetAllCompaniesAsync() =>
            await _context.Companies.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();

        public async Task<Company> GetCompanyByIdAsync(int id) =>
            await _context.Companies.FindAsync(id);

        public async Task<Company> CreateCompanyAsync(Company company)
        {
            company.CreatedAt = System.DateTime.Now;
            company.IsActive = true;
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task UpdateCompanyAsync(Company company)
        {
            var existing = await _context.Companies.FindAsync(company.Id);
            if (existing == null) throw new KeyNotFoundException("Компания не найдена");

            existing.Name = company.Name;
            existing.INN = company.INN;
            existing.OGRN = company.OGRN;
            existing.CompanyType = company.CompanyType;
            existing.Phone = company.Phone;
            existing.Email = company.Email;
            existing.LegalAddress = company.LegalAddress;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCompanyAsync(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                company.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}