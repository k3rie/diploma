using Diploma.Data;
using Diploma.Data.Interfaces;
using Diploma.Models;
using System;
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
            user.UpdatedAt = System.DateTime.Now;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}