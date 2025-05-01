using System;

namespace InventorySystem.Domain.Entities
{
    public class Product
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public long Stock { get; set; }
        public string Category { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}