using Microsoft.OpenApi.Models;
using InventorySystem.Application.Mappings;
using InventorySystem.Application.Features.RabbitMQProducer;
using InventorySystem.Application.Features.RabbitMQProducer.Interfaces;
using InventorySystem.Application.Features.Products.Interfaces;
using InventorySystem.Application.Features.Products;
using InventorySystem.Domain.Interfaces;
using InventorySystem.Infrastructure.Data.Repositories;
using InventorySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using InventorySystem.Domain.Entities;
using InventorySystem.Application.Features.Products.Validations;
using InventorySystem.Application.Features.Inventories.Interfaces;
using InventorySystem.Application.Features.Inventories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Inventory API", Version = "v1" });
});

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddSingleton<IRabbitMQProducer>(sp => 
{
    var logger = sp.GetRequiredService<ILogger<RabbitMQProducer>>();
    var hostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") 
                   ?? builder.Configuration.GetValue<string>("RabbitMQ:Host") 
                   ?? "localhost";
    return new RabbitMQProducer(hostName, logger);
});

builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<IValidator<Product>, ProductValidator>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddTransient<IInventoryService, InventoryService>();
builder.Services.AddScoped<IInventoryRepository,InventoryRepository>();

builder.Services.AddDbContext<ApplicationDbContext>(options => 
{
    var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION") 
                           ?? builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory API v1"));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();