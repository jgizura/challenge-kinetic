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
    }
}