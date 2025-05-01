using InventorySystem.Application.Features.Inventories.Interfaces;
using InventorySystem.Domain.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace InventorySystem.Consumer.Services
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly string _hostName;
        private readonly IInventoryService _inventoryService; 
        private IConnection _connection;
        private IChannel _channel;
        private const string EXCHANGE_NAME = "inventory_exchange";
        private static readonly List<string> QUEUES = new List<string> { "inventory.create", "inventory.update", "inventory.delete" };

        public RabbitMQConsumer(
            string hostName,
            ILogger<RabbitMQConsumer> logger,
            IInventoryService inventoryService
        )
        {
            _hostName = hostName;
            _logger = logger;
            _inventoryService = inventoryService;
            InitializeConnectionAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeConnectionAsync()
        {
            try
            {
                var factory = new ConnectionFactory { HostName = _hostName };
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(
                    exchange: EXCHANGE_NAME,
                    type: "direct",
                    durable: true,
                    autoDelete: false);

                foreach (var queue in QUEUES)
                {
                    await _channel.QueueDeclareAsync(
                        queue: queue,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    var routingKey = queue.Split('.')[1];
                    await _channel.QueueBindAsync(queue, EXCHANGE_NAME, routingKey);
                }

                _logger.LogInformation("RabbitMQ connection established.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
                throw;
            }
        }

        private async Task ProcessMessageAsync(string routingKey, string message)
        {
            try
            {
                var product = JsonSerializer.Deserialize<Product>(message);

                var inventory = new Inventory
                {
                    ProductId = product.Id,
                    Stock = product.Stock,
                    MethodType = routingKey,
                    CreationDate = DateTime.UtcNow
                };
                await _inventoryService.CreateAsync(inventory);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Failed to deserialize message: {message}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                _logger.LogInformation($"Message received from {routingKey}: {message}");

                await ProcessMessageAsync(routingKey, message);
            };

            foreach (var queue in QUEUES)
            {
                await _channel.BasicConsumeAsync(
                    queue: queue,
                    autoAck: true,
                    consumer: consumer);
            }
        }

        public override async void Dispose()
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
                _channel = null;
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
                _connection = null;
            }

            base.Dispose();
        }
    }
}
