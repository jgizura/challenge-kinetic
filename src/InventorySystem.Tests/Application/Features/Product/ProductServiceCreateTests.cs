using AutoMapper;
using FluentAssertions;
using FluentValidation;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Features.Products;
using InventorySystem.Application.Features.RabbitMQProducer.Interfaces;
using InventorySystem.Domain.Interfaces;
using Moq;

namespace InventorySystem.Tests.Application.Features.Product
{
    public class ProductServiceCreateTests
    {
        [Fact]
        public async Task CreateAsync_ShouldCreateProductSuccessfully()
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

            var createProductDto = new CreateProductDto
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 100,
                Stock = 10,
                Category = "Test Category"
            };

            var product = new Domain.Entities.Product
            {
                Id = 1,
                Name = createProductDto.Name,
                Description = createProductDto.Description,
                Price = createProductDto.Price ?? 0,
                Stock = createProductDto.Stock?? 0  ,
                Category = createProductDto.Category
            };

            mapperMock.Setup(m => m.Map<Domain.Entities.Product>(createProductDto)).Returns(product);
            mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Domain.Entities.Product>()))
                .Returns(new ProductDto
                {
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    Category = product.Category
                });
            validatorMock.Setup(v => v.ValidateAsync(product, default)).ReturnsAsync(new FluentValidation.Results.ValidationResult());
             
            // Act
            var result = await productService.CreateAsync(createProductDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createProductDto.Name);
            productRepositoryMock.Verify(r => r.CreateAsync(product), Times.Once);
            messageProducerMock.Verify(m => m.PublishAsync(product, "create"), Times.Once);
        }
    }
}