using System.Threading.Tasks;
using System.Collections.Generic;
using InventorySystem.Domain.Entities;

namespace InventorySystem.Domain.Interfaces
{
    public interface IInventoryRepository
    {
        Task CreateAsync(Inventory inventary);
        Task<IEnumerable<Inventory>> GetAllAsync();
    }
}