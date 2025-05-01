using InventorySystem.Consumer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using InventorySystem.Application.Features.Inventories.Interfaces;
using InventorySystem.Application.Features.Inventories;
using InventorySystem.Domain.Interfaces;
using InventorySystem.Infrastructure.Data.Repositories;

Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;
        var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
                                   ?? configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddTransient<IInventoryService, InventoryService>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();

        services.AddHostedService<RabbitMQConsumer>(rtp =>
        {
            var inventoryService = rtp.GetRequiredService<IInventoryService>();
            var logger = rtp.GetRequiredService<ILogger<RabbitMQConsumer>>();
            var configuration = rtp.GetRequiredService<IConfiguration>();
            var hostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST")
                   ?? configuration.GetValue<string>("RabbitMQ:Host")
                   ?? "localhost";
            return new RabbitMQConsumer(hostName, logger, inventoryService);
        });
    })
    .Build()
    .Run();