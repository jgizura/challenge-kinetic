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
    }
}