using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.Data.Configurations
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            builder.ToTable("Inventories");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.ProductId)
                .IsRequired();

            builder.Property(i => i.Stock)
                .IsRequired();

            builder.Property(i => i.MethodType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(i => i.CreationDate)
                .IsRequired();
        }
    }
}