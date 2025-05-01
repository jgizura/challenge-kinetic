using System;

namespace InventorySystem.Application.DTOs
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public long? Stock { get; set; }
        public string Category { get; set; }
    }
}