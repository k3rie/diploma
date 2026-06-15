using Diploma.Data;
using Diploma.Data.Interfaces;
using Diploma.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Diploma.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DefectDbContext _context;

        public UserRepository(DefectDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username && u.IsActive);
        }

        public async Task<User> GetByUsernameWithCompanyAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.UserName == username && u.IsActive);
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }

        public async Task<bool> ExistsAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.UserName == username && u.IsActive);
        }

        public async Task UpdateAsync(User user)
        {
            var existing = await _context.Users.FindAsync(user.Id);
            if (existing == null) throw new KeyNotFoundException("Пользователь не найден");

            existing.UserName = user.UserName;
            existing.FullName = user.FullName;
            existing.Email = user.Email;
            existing.Phone = user.Phone;
            existing.Role = user.Role;
            existing.CompanyId = user.CompanyId;
            existing.UpdatedAt = DateTime.Now;

            // Пароль обновляем только если передан новый
            if (!string.IsNullOrWhiteSpace(user.PasswordHash) && user.PasswordHash != existing.PasswordHash)
                existing.PasswordHash = user.PasswordHash;

            await _context.SaveChangesAsync();
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.Include(u => u.Company).Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            user.CreatedAt = DateTime.Now;
            user.IsActive = true;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsActive = false;
                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}