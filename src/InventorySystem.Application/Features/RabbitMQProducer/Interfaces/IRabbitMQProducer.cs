using System;
using System.Threading.Tasks;

namespace InventorySystem.Application.Features.RabbitMQProducer.Interfaces
{
    public interface IRabbitMQProducer : IDisposable
    {
        Task PublishAsync<T>(T message, string routingKey);
        Task PublishWithRetryAsync<T>(T message, string routingKey, int maxRetries = 3);
        bool IsConnectionOpen();
        void ReconnectIfNeeded();
    }
}