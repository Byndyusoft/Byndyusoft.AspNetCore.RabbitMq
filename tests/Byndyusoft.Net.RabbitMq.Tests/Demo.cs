﻿using Byndyusoft.Net.RabbitMq.Abstractions;
using Byndyusoft.Net.RabbitMq.Extensions;
using Byndyusoft.Net.RabbitMq.Services;
using Byndyusoft.Net.RabbitMq.Services.Pipes;
using Byndyusoft.Net.RabbitMq.Services.Wrappers;
using Microsoft.Extensions.DependencyInjection;

namespace Byndyusoft.Net.RabbitMq.Tests
{
    public class Demo
    {
        public void Main()
        {
            var serviceCollection = new ServiceCollection();
            var serviceProvider =
            serviceCollection.AddRabbitMq(
                configurator => configurator.Connection("localhost")
                    .Exchange("incoming-documents",
                        exchangeConfigurator =>
                        {
                            exchangeConfigurator.Consume<RawDocument>("raw_documents", "raw")
                                .Wrap<TracerConsumeWrapper<RawDocument>>()
                                .PipeError<PushToErrorQueue<RawDocument>>();


                            exchangeConfigurator.Produce<EnrichedDocument>("enriched_documents", "enriched")
                                .Wrap<TracerProduceWrapper<EnrichedDocument>>()
                                .PipeReturned<TraceReturned<EnrichedDocument>>();
                        })).BuildServiceProvider();



            var queueService = serviceProvider.GetRequiredService<IQueueService>();
            queueService.SubscribeAsync<RawDocument>(async raw =>
            {
                var enriched = new EnrichedDocument
                {
                    RawDocument = raw
                };

                await queueService.Publish(enriched);
            });
        }
    }
}
