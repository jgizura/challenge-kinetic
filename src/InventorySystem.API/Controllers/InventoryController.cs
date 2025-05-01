using Microsoft.AspNetCore.Mvc;
using InventorySystem.Application.Features.Inventories.Interfaces;
using InventorySystem.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventorySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// Retrieves all inventory items
        /// </summary>
        /// <returns>A collection of inventory items</returns>
        /// <response code="200">Returns the list of inventory items</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Inventory>))]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetAll()
        {
            var inventories = await _inventoryService.GetAllAsync();
            return Ok(inventories);
        }
    }
}