using InventorySystem.Application.Features.ResiliencePolicies;
using Microsoft.Extensions.Logging;
using Moq;
using Polly.CircuitBreaker;
using InventorySystem.Domain.Interfaces;
using InventorySystem.Application.Features.RabbitMQProducer.Interfaces;
using Polly;

namespace InventorySystem.Tests
{
    public class ResiliencePoliciesTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public ResiliencePoliciesTests()
        {
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public async Task CircuitBreakerPolicy_ShouldOpenAfterThreshold()
        {
            // Arrange
            var policy = ResiliencePolicies.CircuitBreakerPolicy;

            // Act
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await policy.ExecuteAsync(() => throw new Exception("Test exception"));
                }
                catch (Exception)
                {
                    // Ignore exceptions until the circuit breaker opens
                }
            }

            // Assert
            await Assert.ThrowsAsync<BrokenCircuitException>(async () =>
            {
                await policy.ExecuteAsync(() => Task.CompletedTask);
            });
        }

        [Fact]
        public async Task RetryPolicy_ShouldRetrySpecifiedNumberOfTimes_WithMockedRabbitMQProducer()
        {
            // Arrange
            var policy = ResiliencePolicies.RetryPolicy;
            int retryCount = 0;

            var mockRabbitMQProducer = new Mock<IRabbitMQProducer>();
            mockRabbitMQProducer.Setup(p => p.IsConnectionOpen()).Returns(true);
            mockRabbitMQProducer.Setup(p => p.PublishAsync(It.IsAny<object>(), It.IsAny<string>()))
                .Throws(new Exception("Simulated exception"));

            var context = new Context();
            context["RabbitMQProducer"] = mockRabbitMQProducer.Object;
            context["Message"] = "Test message";
            context["RoutingKey"] = "TestKey";

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                // Incrementar el contador en cada ejecución, incluyendo el intento inicial
                await policy.ExecuteAsync(async (ctx) =>
                {
                    retryCount++;

                    if (ctx is Context pollyContext)
                    {
                        var producer = pollyContext["RabbitMQProducer"] as IRabbitMQProducer;
                        if (producer == null)
                        {
                            Console.WriteLine("No se encontró un productor de RabbitMQ en el contexto.");
                            throw new Exception("RabbitMQProducer not found");
                        }

                        var message = pollyContext["Message"] as string;
                        var routingKey = pollyContext["RoutingKey"] as string;

                        if (message == null || routingKey == null)
                        {
                            throw new Exception("Message or RoutingKey is not properly set in the context.");
                        }

                        await producer.PublishAsync(message, routingKey);
                    }
                    else
                    {
                        throw new Exception("Invalid context type.");
                    }
                }, context);
            });

            // Assert
            Assert.Equal("Simulated exception", exception.Message);
            Assert.Equal(3, retryCount);
        }
    }
}