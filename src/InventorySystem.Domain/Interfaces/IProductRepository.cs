using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using InventorySystem.Domain.Entities;

namespace InventorySystem.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(long id);
        Task CreateAsync(Product product);
        Task UpdateAsync(Product product);

        Task<bool> ValidateUniqueAsync(Expression<Func<Product, bool>> where);
    }
}