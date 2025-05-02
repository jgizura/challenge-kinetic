using AutoMapper;
using FluentValidation;
using InventorySystem.Application.Features.Products;
using InventorySystem.Application.Features.RabbitMQProducer.Interfaces;
using InventorySystem.Domain.Interfaces;
using Moq;

namespace InventorySystem.Tests.Application.Features.Product
{
    public class ProductServiceDeleteTests
    {
        [Fact]
        public async Task DeleteAsync_ShouldDeleteProductSuccessfully()
        {
            // Arrange
            var productRepositoryMock = new Mock<IProductRepository>();
            var messageProducerMock = new Mock<IRabbitMQProducer>();
            var mapperMock = new Mock<IMapper>();
            var validatorMock = new Mock<IValidator<Domain.Entities.Product>>();

            var productService = new ProductService(
                productRepositoryMock.Object,
                messageProducerMock.Object,
                mapperMock.Object,
                validatorMock.Object
            );

            var existingProduct = new Domain.Entities.Product
            {
                Id = 1,
                Name = "Test Product",
                Description = "Test Description",
                Price = 100,
                Stock = 10,
                Category = "Test Category"
            };

            productRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingProduct);
            validatorMock.Setup(v => v.ValidateAsync(existingProduct, default)).ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // Act
            await productService.DeleteAsync(1);

            // Assert
            productRepositoryMock.Verify(r => r.UpdateAsync(existingProduct), Times.Once);
            messageProducerMock.Verify(m => m.PublishAsync(existingProduct, "delete"), Times.Once);
        }
    }
}