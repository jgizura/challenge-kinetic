using System;
using System.Threading.Tasks;

namespace InventorySystem.Application.Features.RabbitMQProducer.Interfaces
{
    public interface IRabbitMQProducer : IDisposable
    {
        Task PublishAsync<T>(T message, string routingKey);
        bool IsConnectionOpen();
        void IncrementConnectionFailureCount();
        void ReconnectIfNeeded();
    }
}