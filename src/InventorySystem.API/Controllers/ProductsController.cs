using Microsoft.AspNetCore.Mvc;
using InventorySystem.Application.DTOs;
using System.ComponentModel.DataAnnotations;
using InventorySystem.Application.Features.Products.Interfaces;

namespace InventorySystem.API.Controllers
{
    /// <summary>
    /// API controller for managing inventory products
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Retrieves all products from the inventory
        /// </summary>
        /// <returns>A collection of products</returns>
        /// <response code="200">Returns the list of products</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDto>))]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        /// <summary>
        /// Retrieves a specific product by its ID
        /// </summary>
        /// <param name="id">The ID of the product to retrieve</param>
        /// <returns>The requested product</returns>
        /// <response code="200">Returns the requested product</response>
        /// <response code="404">If the product is not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetProductById([Required] long id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product is null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        /// <summary>
        /// Creates a new product in the inventory
        /// </summary>
        /// <param name="createProductDto">The product information</param>
        /// <returns>The newly created product</returns>
        /// <response code="201">Returns the newly created product</response>
        /// <response code="400">If the request data is invalid</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProductDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> CreateProduct([Required] CreateProductDto createProductDto)
        {
            return await _productService.CreateAsync(createProductDto);
        }

        /// <summary>
        /// Updates an existing product
        /// </summary>
        /// <param name="id">The ID of the product to update</param>
        /// <param name="updateProductDto">The updated product information</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">If the product was updated successfully</response>
        /// <response code="400">If the ID in the URL doesn't match the body or data is invalid</response>
        /// <response code="404">If the product is not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateProduct([Required] long id, [Required] CreateProductDto updateProductDto)
        {
            await _productService.UpdateAsync(id, updateProductDto);
            return NoContent();
        }

        /// <summary>
        /// Deletes a product from the inventory
        /// </summary>
        /// <param name="id">The ID of the product to delete</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">If the product was deleted successfully</response>
        /// <response code="404">If the product is not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteProduct([Required] long id)
        {
            await _productService.DeleteAsync(id);
            return NoContent();
        }
    }
}