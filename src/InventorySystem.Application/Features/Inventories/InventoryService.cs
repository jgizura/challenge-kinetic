using System.Collections.Generic;
using System.Threading.Tasks;
using InventorySystem.Application.Features.Inventories.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Interfaces;

namespace InventorySystem.Application.Features.Inventories
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task CreateAsync(Inventory inventary)
        {
            await _inventoryRepository.CreateAsync(inventary);
        }

        public async Task<IEnumerable<Inventory>> GetAllAsync()
        {
            return await _inventoryRepository.GetAllAsync();
        }
    }
}