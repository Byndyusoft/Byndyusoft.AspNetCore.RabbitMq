using System;
using System.Threading;
using System.Threading.Tasks;
using Byndyusoft.Messaging.RabbitMq.Topology;

namespace Byndyusoft.Messaging.RabbitMq.Abstractions
{
    public interface IRabbitMqClient : IDisposable
    {
        RabbitMqClientOptions Options { get; }

        Task<ReceivedRabbitMqMessage?> GetMessageAsync(string queueName, CancellationToken cancellationToken = default);

        Task AckAsync(ReceivedRabbitMqMessage message, CancellationToken cancellationToken = default);

        Task NackAsync(ReceivedRabbitMqMessage message, bool requeue = false,
            CancellationToken cancellationToken = default);

        Task CompleteMessageAsync(ReceivedRabbitMqMessage message, ConsumeResult consumeResult,
            CancellationToken cancellationToken = default);

        Task PublishMessageAsync(RabbitMqMessage message, CancellationToken cancellationToken = default);

        Task CreateQueueAsync(string queueName,
            QueueOptions options,
            CancellationToken cancellationToken = default);

        Task<bool> QueueExistsAsync(string queueName, CancellationToken cancellationToken = default);

        Task DeleteQueueAsync(string queueName,
            bool ifUnused = false,
            bool ifEmpty = false,
            CancellationToken cancellationToken = default);

        Task PurgeQueueAsync(string queueName, CancellationToken cancellationToken = default);

        Task<ulong> GetQueueMessageCountAsync(string queueName, CancellationToken cancellationToken = default);

        Task CreateExchangeAsync(string exchangeName,
            ExchangeOptions options,
            CancellationToken cancellationToken = default);

        Task<bool> ExchangeExistsAsync(string exchangeName, CancellationToken cancellationToken = default);

        Task DeleteExchangeAsync(string exchangeName,
            bool ifUnused = false,
            CancellationToken cancellationToken = default);

        Task BindQueueAsync(string exchangeName,
            string routingKey,
            string queueName,
            CancellationToken cancellationToken = default);

        RabbitMqConsumer Subscribe(string queueName,
            Func<ReceivedRabbitMqMessage, CancellationToken, Task<ConsumeResult>> onMessage);
    }
}