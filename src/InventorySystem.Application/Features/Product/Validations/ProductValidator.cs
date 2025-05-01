
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Interfaces;

namespace InventorySystem.Application.Features.Products.Validations
{
    public class ProductValidator : AbstractValidator<Product>
    {
        private readonly IProductRepository _productRepository;

        public ProductValidator(IProductRepository productRepository)
        {
            _productRepository = productRepository;
            RuleFor(product => product).CustomAsync(ValidateUniqueAsync);
            RuleFor(product => product).Custom(ValidatePrice);

            RuleSet("delete", () =>
            {
                RuleFor(x => x).Custom(ValidateStock);
            });
        }

        private async Task ValidateUniqueAsync(Product product, ValidationContext<Product> context, CancellationToken token)
        {
            var productFoundByName = await _productRepository.ValidateUniqueAsync(t =>
                                                (string.IsNullOrEmpty(product.Name) || t.Name.ToLower() == product.Name.ToLower()) &&
                                                (t.Id == 0 || t.Id != product.Id));

            if (productFoundByName && !string.IsNullOrEmpty(product.Name))
            {
                context.AddFailure(nameof(product.Name), $"Product with name {product.Name} already exists.");
            }
        }

        private void ValidatePrice(Product product, ValidationContext<Product> context)
        {
            if (product.Price < 1)
            {
                context.AddFailure(nameof(product.Price), "Price must be greater than or equal to 0.");
            }
        }

        private void ValidateStock(Product product, ValidationContext<Product> context)
        {
            if (product.Stock > 0)
            {
                context.AddFailure(nameof(product.Name), "Product has stock movement. Cannot delete.");
            }

        }
    }
}