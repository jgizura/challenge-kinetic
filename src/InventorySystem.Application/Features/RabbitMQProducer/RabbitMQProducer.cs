using InventorySystem.Application.Features.RabbitMQProducer.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
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

        private static readonly AsyncCircuitBreakerPolicy _circuitBreaker = Policy
            .Handle<Exception>() // Maneja cualquier excepción que ocurra.
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3, // Número de excepciones permitidas antes de abrir el circuito.
                durationOfBreak: TimeSpan.FromMinutes(1), // Tiempo que el circuito permanecerá abierto antes de intentar restablecerse.
                onBreak: (ex, breakDelay) =>
                {
                    // Log para indicar que el circuito se ha abierto debido a múltiples fallos.
                },
                onReset: () =>
                {
                    // Log para indicar que el circuito se ha restablecido y está listo para operar nuevamente.
                },
                onHalfOpen: () =>
                {
                    // Log para indicar que el circuito está en estado "half-open" y está probando si puede restablecerse.
                }
            );

        private static readonly AsyncRetryPolicy _retryPolicy = Policy
            .Handle<Exception>() // Maneja cualquier excepción que ocurra durante la operación.
            .WaitAndRetryAsync(
                retryCount: 3, // Número máximo de intentos de reintento.
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), // Tiempo de espera exponencial entre intentos.
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    // Log para registrar cada intento de reintento, incluyendo la excepción y el tiempo de espera.
                }
            );

        public RabbitMQProducer(string hostName, ILogger<RabbitMQProducer> logger)
        {
            _hostName = hostName;
            _logger = logger;
            InitializeConnectionAsync().GetAwaiter().GetResult();
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
                throw;
            }
        }

        public async Task PublishAsync<T>(T message, string routingKey)
        {
            await Task.Run(async () =>
            {
                ReconnectIfNeeded();

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
                    throw;
                }
            });
        }

        public async Task PublishWithRetryAsync<T>(T message, string routingKey, int maxRetries = 3)
        {
            await _circuitBreaker.ExecuteAsync(async () =>
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await PublishAsync(message, routingKey);
                });
            });
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