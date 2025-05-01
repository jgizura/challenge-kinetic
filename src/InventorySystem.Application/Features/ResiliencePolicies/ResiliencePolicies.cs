using InventorySystem.Application.Features.RabbitMQProducer.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System;

namespace InventorySystem.Application.Features.ResiliencePolicies
{
    public static class ResiliencePolicies
    {
        private static readonly ILogger _logger;

        static ResiliencePolicies()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactory.CreateLogger("ResiliencePolicies");
        }

        public static readonly AsyncCircuitBreakerPolicy CircuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (ex, breakDelay) =>
                {
                    _logger.LogError(ex, "El disyuntor se abrió debido a: {Message}. Duración de la pausa: {BreakDelay}", ex.Message, breakDelay);
                },
                onReset: () =>
                {
                    _logger.LogInformation("El disyuntor se ha restablecido. El sistema está saludable nuevamente.");
                },
                onHalfOpen: () =>
                {
                    _logger.LogWarning("El disyuntor está medio abierto. Probando la salud del sistema.");
                });

        public static readonly AsyncRetryPolicy RetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: async (exception, timeSpan, retryAttempt, context) =>
                {
                    Console.WriteLine($"Reintento {retryAttempt} después de {timeSpan.TotalSeconds} segundos debido a: {exception.Message}");

                    if (retryAttempt < 3 && context.TryGetValue("RabbitMQProducer", out var producerObj) && producerObj is IRabbitMQProducer producer)
                    {
                        if (producer.IsConnectionOpen()) // Verificar si RabbitMQ está disponible
                        {
                            try
                            {
                                // Reintentar publicar el mensaje en RabbitMQ
                                var message = context["Message"];
                                var routingKey = context["RoutingKey"].ToString();
                                await producer.PublishAsync(message, routingKey);
                                Console.WriteLine("Mensaje republicado exitosamente en RabbitMQ.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error al republicar en RabbitMQ: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("RabbitMQ no está disponible. No se intentará republicar.");
                            producer.IncrementConnectionFailureCount(); // Incrementar el contador de fallos de conexión
                        }
                    }
                });
    }
}