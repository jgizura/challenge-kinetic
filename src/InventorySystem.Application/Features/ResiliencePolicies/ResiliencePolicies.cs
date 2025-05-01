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
                onRetry: async (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Intento de reintento {retryCount} después de {timeSpan.TotalSeconds} segundos debido a: {exception.Message}");

                    if (retryCount <= 3) // Solo intentar republicar dentro de los primeros 3 intentos
                    {
                        if (context.TryGetValue("RabbitMQProducer", out var producerObj) && producerObj is IRabbitMQProducer producer)
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
                        else
                        {
                            Console.WriteLine("No se encontró un productor de RabbitMQ en el contexto.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Se superó el límite de intentos para republicar en RabbitMQ.");
                    }

                    // Opcionalmente, actualizar métricas en un sistema de monitoreo
                    // Opcionalmente, enviar una alerta si retryCount supera un umbral
                });
    }
}