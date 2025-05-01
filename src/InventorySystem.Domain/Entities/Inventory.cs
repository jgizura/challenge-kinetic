using System;

namespace InventorySystem.Domain.Entities
{
    public class Inventory
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long Stock { get; set; }
        public string MethodType { get; set; }
        public DateTime CreationDate { get; set; }
    }
}