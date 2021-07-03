﻿using System;
using System.Threading.Tasks;
using Byndyusoft.Net.RabbitMq.Abstractions;
using Byndyusoft.Net.RabbitMq.Models;
using EasyNetQ;
using Newtonsoft.Json;
using OpenTracing;
using OpenTracing.Propagation;

namespace Byndyusoft.Net.RabbitMq.Services
{
    public sealed class TracerProduceWrapper<TMessage> : IProduceWrapper<TMessage> where TMessage : class
    {
        private readonly ITracer _tracer;

        public TracerProduceWrapper(ITracer tracer)
        {
            _tracer = tracer;
        }

        public async Task WrapPipe(IMessage<TMessage> message, IProducePipe<TMessage> pipe)
        {
            if (_tracer.ActiveSpan == null)
                throw new InvalidOperationException("No active tracing span. Push to queue will broken service chain");

            var span = _tracer.BuildSpan(nameof(WrapPipe)).Start();

            span.SetTag(nameof(message), JsonConvert.SerializeObject(message));

            var carrier = new HttpHeadersCarrier(message.Properties.Headers);

            _tracer.Inject(_tracer.ActiveSpan.Context, BuiltinFormats.HttpHeaders, carrier);

            await pipe.Pipe(message);

            span.Finish();
        }
    }
}