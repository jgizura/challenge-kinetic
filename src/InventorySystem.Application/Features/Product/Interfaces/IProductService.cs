using System.Collections.Generic;
using System.Threading.Tasks;
using InventorySystem.Application.DTOs;

namespace InventorySystem.Application.Features.Products.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto> GetByIdAsync(long id);
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto> CreateAsync(CreateProductDto createProductDto);
        Task<ProductDto> UpdateAsync(long id, CreateProductDto updateProductDto);
        Task DeleteAsync(long id);
    }
}