using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Data.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Where(x => !x.DeletedDate.HasValue)
                .ToListAsync();
        }

        public async Task<Product> GetByIdAsync(long id)
        {
            return await _context.Products
                .FirstOrDefaultAsync(x => x.Id == id && !x.DeletedDate.HasValue);
        }

        public async Task CreateAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public Task<bool> ValidateUniqueAsync(Expression<Func<Product, bool>> where)
        {
            return _context.Products.AsNoTracking().AnyAsync(where);
        }
    }
}