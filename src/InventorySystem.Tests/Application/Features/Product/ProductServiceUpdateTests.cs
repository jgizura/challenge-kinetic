using AutoMapper;
using FluentValidation;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Features.Products;
using InventorySystem.Application.Features.RabbitMQProducer.Interfaces;
using InventorySystem.Domain.Interfaces;
using FluentAssertions;
using Moq;
using InventorySystem.Domain.Entities;

namespace InventorySystem.Tests.Application.Features.Product
{
    public class ProductServiceUpdateTests
    {
        [Fact]
        public async Task UpdateAsync_ShouldUpdateProductSuccessfully()
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

            var updateProductDto = new CreateProductDto
            {
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 200,
                Stock = 20,
                Category = "Updated Category"
            };

            var existingProduct = new Domain.Entities.Product
            {
                Id = 1,
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 100,
                Stock = 10,
                Category = "Old Category"
            };

            productRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingProduct);
            validatorMock.Setup(v => v.ValidateAsync(existingProduct, default)).ReturnsAsync(new FluentValidation.Results.ValidationResult());
            mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Domain.Entities.Product>()))
               .Returns(new ProductDto
               {
                   Name = existingProduct.Name,
                   Description = existingProduct.Description,
                   Price = existingProduct.Price,
                   Stock = existingProduct.Stock,
                   Category = existingProduct.Category
               });

            // Act
            var result = await productService.UpdateAsync(1, updateProductDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(updateProductDto.Name);
            productRepositoryMock.Verify(r => r.UpdateAsync(existingProduct), Times.Once);
            messageProducerMock.Verify(m => m.PublishAsync(existingProduct, "update"), Times.Once);
        }
    }
}