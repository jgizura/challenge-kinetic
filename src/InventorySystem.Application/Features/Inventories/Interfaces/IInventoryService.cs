using System.Threading.Tasks;
using System.Collections.Generic;
using InventorySystem.Domain.Entities;

namespace InventorySystem.Application.Features.Inventories.Interfaces
{
    public interface IInventoryService
    {
        Task CreateAsync(Inventory inventary);
        Task<IEnumerable<Inventory>> GetAllAsync();
    }
}