using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Description)
                .HasMaxLength(500);

            builder.Property(p => p.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Stock)
                .IsRequired();

            builder.Property(p => p.Category)
                .HasMaxLength(50);

            builder.Property(p => p.CreationDate)
                .IsRequired();

            builder.Property(p => p.LastUpdateDate)
                .IsRequired(false);

            builder.Property(p => p.DeletedDate)
                .IsRequired(false);
        }
    }
}