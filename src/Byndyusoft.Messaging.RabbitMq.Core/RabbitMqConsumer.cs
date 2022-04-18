using System;
using System.Threading;
using System.Threading.Tasks;
using Byndyusoft.Messaging.RabbitMq.Abstractions;
using Byndyusoft.Messaging.RabbitMq.Abstractions.Utils;

namespace Byndyusoft.Messaging.RabbitMq.Core
{


    public class RabbitMqConsumer : Disposable, IRabbitMqConsumer
    {
        private readonly IRabbitMqClientHandler _handler;
        private ReceivedRabbitMqMessageHandler _onMessage;
        private readonly string _queueName;

        private IDisposable? _consumer;
        private bool? _exclusive;
        private ushort? _prefetchCount;

        public RabbitMqConsumer(IRabbitMqClient client,
            IRabbitMqClientHandler handler,
            string queueName,
            ReceivedRabbitMqMessageHandler onMessage)
        {
            Client = client;
            _handler = handler;
            _onMessage = onMessage;
            _queueName = queueName;
        }

        public bool IsRunning => _consumer is not null;

        public string QueueName
        {
            get
            {
                Preconditions.CheckNotDisposed(this);
                return _queueName;
            }
        }

        public bool? Exclusive
        {
            get
            {
                Preconditions.CheckNotDisposed(this);
                return _exclusive;
            }
            set
            {
                Preconditions.Check(IsRunning == false, "Can't change exclusive mode for started consumer");

                _exclusive = value;
            }
        }

        public ushort? PrefetchCount
        {
            get
            {
                Preconditions.CheckNotDisposed(this);
                return _prefetchCount;
            }
            set
            {
                Preconditions.Check(IsRunning == false, "Can't change prefetch count for started consumer");

                _prefetchCount = value;
            }
        }


        public event BeforeRabbitQueueConsumerStartEventHandler? OnStarting;

        public event AfterRabbitQueueConsumerStopEventHandler? OnStopped;

        public IRabbitMqClient Client { get; }

        public ReceivedRabbitMqMessageHandler OnMessage
        {
            get => _onMessage;
            set => _onMessage = Preconditions.CheckNotNull(value, nameof(OnMessage));
        }

        public async Task<IRabbitMqConsumer> StartAsync(CancellationToken cancellationToken = default)
        {
            Preconditions.CheckNotDisposed(this);

            if (IsRunning)
                return this;

            if(OnStarting != null)
                await OnStarting(this, cancellationToken).ConfigureAwait(false);

            async Task<HandlerConsumeResult> OnMessage(ReceivedRabbitMqMessage message, CancellationToken token)
            {
                try
                {
                    try
                    {
                        var consumeResult = await _onMessage(message, token).ConfigureAwait(false);
                        return await HandleConsumeResultAsync(message, consumeResult, token).ConfigureAwait(false);
                    }
                    catch (Exception exception)
                    {
                        await _handler.PublishMessageToErrorQueueAsync(message, Client.Options.NamingConventions, exception, cancellationToken)
                            .ConfigureAwait(false);
                        return HandlerConsumeResult.Ack;
                    }
                }
                catch
                {
                    return HandlerConsumeResult.RejectWithRequeue;
                }
            }

            _consumer = _handler.StartConsume(QueueName, _exclusive, _prefetchCount, OnMessage);

            return this;
        }

        public async Task<IRabbitMqConsumer> StopAsync(CancellationToken cancellationToken = default)
        {
            Preconditions.CheckNotDisposed(this);

            if (IsRunning == false)
                return this;

            if(OnStopped != null)
                await OnStopped(this, cancellationToken).ConfigureAwait(false);

            _consumer?.Dispose();
            _consumer = null;

            return this;
        }

        protected override void DisposeCore()
        {
            _consumer?.Dispose();
            _consumer = null;

            base.DisposeCore();
        }

        private async Task<HandlerConsumeResult> HandleConsumeResultAsync(ReceivedRabbitMqMessage consumedMessage,
            ConsumeResult consumeResult, CancellationToken cancellationToken)
        {
            switch (consumeResult)
            {
                case AckConsumeResult:
                    return HandlerConsumeResult.Ack;

                case RejectWithRequeueConsumeResult:
                    return HandlerConsumeResult.RejectWithRequeue;

                case RejectWithoutRequeueConsumeResult:
                    return HandlerConsumeResult.RejectWithoutRequeue;
                
                case ErrorConsumeResult error:
                    await _handler.PublishMessageToErrorQueueAsync(consumedMessage, Client.Options.NamingConventions, error.Exception, cancellationToken)
                        .ConfigureAwait(false);
                    return HandlerConsumeResult.Ack;

                default:
                    throw new ArgumentOutOfRangeException(nameof(consumeResult), consumeResult, null);
            }
        }
    }
}