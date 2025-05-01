using InventorySystem.Application.Features.RabbitMQProducer.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InventorySystem.Application.Features.RabbitMQProducer
{
    public class RabbitMQProducer : IRabbitMQProducer
    {
        private readonly string _hostName;
        private readonly string _exchangeName = "inventory_exchange";
        private IConnection _connection;
        private IChannel _channel;
        private readonly ILogger<RabbitMQProducer> _logger;

        private static readonly List<string> QUEUES = new List<string> { "inventory.create", "inventory.update", "inventory.delete" };

        private int _connectionFailureCount = 0;

        public int ConnectionFailureCount => _connectionFailureCount;

        public RabbitMQProducer(string hostName, ILogger<RabbitMQProducer> logger)
        {
            _hostName = hostName;
            _logger = logger;
            InitializeConnectionAsync().GetAwaiter().GetResult();
        }

        public void IncrementConnectionFailureCount()
        {
            _connectionFailureCount++;
            _logger.LogWarning("Failed to connect to RabbitMQ. Attempt count: {FailureCount}", _connectionFailureCount);
        }

        private async Task InitializeConnectionAsync()
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = _hostName };
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(
                    exchange: _exchangeName,
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
                    await _channel.QueueBindAsync(queue, _exchangeName, routingKey);
                }

                _connectionFailureCount = 0;
            }
            catch (Exception ex)
            {
                IncrementConnectionFailureCount();
                _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
                throw;
            }
        }

        public async Task PublishAsync<T>(T message, string routingKey)
        {
            if (!IsConnectionOpen())
            {
                _logger.LogError("Cannot publish message. RabbitMQ connection is not open.");
                return;
            }

            try
            {
                var jsonMessage = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(jsonMessage);

                await _channel.BasicPublishAsync(
                    exchange: _exchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    body: body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message with routing key {RoutingKey}", routingKey);
            }
        }

        public bool IsConnectionOpen()
        {
            return _connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen;
        }

        public void ReconnectIfNeeded()
        {
            if (!IsConnectionOpen())
            {
                _logger.LogInformation("Reconnecting to RabbitMQ...");
                try
                {
                    Dispose();
                    InitializeConnectionAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to reconnect to RabbitMQ");
                    throw;
                }
            }
        }

        public async void Dispose()
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
        }
    }
}