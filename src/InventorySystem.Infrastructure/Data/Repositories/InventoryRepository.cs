using System.Collections.Generic;
using System.Threading.Tasks;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Data.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly ApplicationDbContext _context;

        public InventoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Inventory inventary)
        {
            await _context.Inventories.AddAsync(inventary);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Inventory>> GetAllAsync()
        {
            return await _context.Inventories.ToListAsync();
        }
    }
}