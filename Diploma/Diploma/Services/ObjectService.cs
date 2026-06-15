using Diploma.Data;
using Diploma.Models;
using Diploma.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Diploma.Services
{
    public class ObjectService : IObjectService
    {
        private readonly DefectDbContext _context;

        public ObjectService(DefectDbContext context) => _context = context;

        // Объекты
        public async Task<List<ConstructionObject>> GetAllObjectsAsync() =>
            await _context.ConstructionObjects
                .Include(o => o.DeveloperCompany)
                .OrderBy(o => o.Name)
                .ToListAsync();

        public async Task<ConstructionObject> GetObjectByIdAsync(int id) =>
            await _context.ConstructionObjects
                .Include(o => o.DeveloperCompany)
                .FirstOrDefaultAsync(o => o.Id == id);

        public async Task<ConstructionObject> CreateObjectAsync(ConstructionObject obj)
        {
            obj.CreatedAt = System.DateTime.Now;
            _context.ConstructionObjects.Add(obj);
            await _context.SaveChangesAsync();
            return obj;
        }

        public async Task UpdateObjectAsync(ConstructionObject obj)
        {
            var existing = await _context.ConstructionObjects.FindAsync(obj.Id);
            if (existing == null) throw new KeyNotFoundException("Объект не найден");

            existing.Name = obj.Name;
            existing.Address = obj.Address;
            existing.DeveloperCompanyId = obj.DeveloperCompanyId;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteObjectAsync(int id)
        {
            var obj = await _context.ConstructionObjects.FindAsync(id);
            if (obj != null)
            {
                _context.ConstructionObjects.Remove(obj); // каскадное удаление помещений
                await _context.SaveChangesAsync();
            }
        }

        // Помещения
        public async Task<List<Premise>> GetPremisesByObjectAsync(int objectId) =>
            await _context.Premises
                .Where(p => p.ObjectId == objectId)
                .Include(p => p.Owner)
                .OrderBy(p => p.Floor).ThenBy(p => p.Number)
                .ToListAsync();

        public async Task<Premise> GetPremiseByIdAsync(int premiseId) =>
            await _context.Premises.Include(p => p.ConstructionObject).Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == premiseId);

        public async Task<Premise> CreatePremiseAsync(Premise premise)
        {
            premise.CreatedAt = System.DateTime.Now;
            _context.Premises.Add(premise);
            await _context.SaveChangesAsync();
            return premise;
        }

        public async Task UpdatePremiseAsync(Premise premise)
        {
            var existing = await _context.Premises.FindAsync(premise.Id);
            if (existing == null) throw new KeyNotFoundException("Помещение не найдено");

            existing.Number = premise.Number;
            existing.Floor = premise.Floor;
            existing.Area = premise.Area;
            existing.OwnerId = premise.OwnerId;
            // CreatedAt не трогаем

            await _context.SaveChangesAsync();
        }

        public async Task DeletePremiseAsync(int premiseId)
        {
            var premise = await _context.Premises.FindAsync(premiseId);
            if (premise != null)
            {
                _context.Premises.Remove(premise);
                await _context.SaveChangesAsync();
            }
        }
    }
}