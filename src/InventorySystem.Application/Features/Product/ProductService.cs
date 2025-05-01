using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Features.Products.Interfaces;
using InventorySystem.Application.Features.RabbitMQProducer.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Interfaces;

namespace InventorySystem.Application.Features.Products
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IRabbitMQProducer _messageProducer;
        private readonly IMapper _mapper;
        private readonly IValidator<Product> _createProductDtoValidator;

        private const string CREATE_ROUTING_KEY = "create";
        private const string UPDATE_ROUTING_KEY = "update";
        private const string DELETE_ROUTING_KEY = "delete";

        public ProductService(
            IProductRepository productRepository,
            IRabbitMQProducer messageProducer,
            IMapper mapper,
            IValidator<Product> createProductDtoValidator
        )
        {
            _productRepository = productRepository;
            _messageProducer = messageProducer;
            _mapper = mapper;
            _createProductDtoValidator = createProductDtoValidator;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> GetByIdAsync(long id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto)
        {
            var product = _mapper.Map<Product>(createProductDto);
            var validationResult = await _createProductDtoValidator.ValidateAsync(product);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            await _productRepository.CreateAsync(product);
            await _messageProducer.PublishWithRetryAsync(product, CREATE_ROUTING_KEY);

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> UpdateAsync(long id, CreateProductDto updateProductDto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product is null) throw new ValidationException("Not found");

            product.Name = updateProductDto.Name ?? product.Name;
            product.Description = updateProductDto.Description ?? product.Description;
            product.Price = updateProductDto.Price ?? product.Price;
            product.Stock = updateProductDto.Stock ?? product.Stock;
            product.Category = updateProductDto.Category ?? product.Category;
            product.LastUpdateDate = DateTime.UtcNow;

            var validationResult = await _createProductDtoValidator.ValidateAsync(product);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            await _productRepository.UpdateAsync(product);
            await _messageProducer.PublishWithRetryAsync(product, UPDATE_ROUTING_KEY);

            return _mapper.Map<ProductDto>(product);
        }

        public async Task DeleteAsync(long id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product is null) throw new ValidationException("Not found");

            var validationResult = await _createProductDtoValidator.ValidateAsync(product, options =>
                        {
                            options.IncludeRuleSets("delete");
                        });
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            product.DeletedDate = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            await _messageProducer.PublishWithRetryAsync(product, DELETE_ROUTING_KEY);
        }
    }
}