using Diploma.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Diploma.Services.Interfaces
{
    public interface IObjectService
    {
        Task<List<ConstructionObject>> GetAllObjectsAsync();
        Task<ConstructionObject> GetObjectByIdAsync(int id);
        Task<ConstructionObject> CreateObjectAsync(ConstructionObject obj);
        Task UpdateObjectAsync(ConstructionObject obj);
        Task DeleteObjectAsync(int id);

        Task<List<Premise>> GetPremisesByObjectAsync(int objectId);
        Task<Premise> GetPremiseByIdAsync(int premiseId);
        Task<Premise> CreatePremiseAsync(Premise premise);
        Task UpdatePremiseAsync(Premise premise);
        Task DeletePremiseAsync(int premiseId);
    }
}