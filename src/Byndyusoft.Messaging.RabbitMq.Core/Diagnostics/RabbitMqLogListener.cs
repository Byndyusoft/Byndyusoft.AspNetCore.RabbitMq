using System.Collections.Generic;
using System.Text;
using Byndyusoft.Messaging.RabbitMq.Diagnostics.Base;
using Byndyusoft.Messaging.RabbitMq.Diagnostics.Builders;
using Byndyusoft.Messaging.RabbitMq.Diagnostics.Consts;
using Microsoft.Extensions.Logging;

namespace Byndyusoft.Messaging.RabbitMq.Diagnostics
{
    public class RabbitMqLogListener : ListenerHandler
    {
        private readonly ILogger<RabbitMqLogListener> _logger;
        private readonly RabbitMqClientCoreOptions _options;

        public RabbitMqLogListener(
            ILogger<RabbitMqLogListener> logger,
            RabbitMqClientCoreOptions options) 
            : base(DiagnosticNames.RabbitMq)
        {
            _logger = logger;
            _options = options;
        }

        public override bool SupportsNullActivity => true;

        public override void OnEventWritten(string name, object? payload)
        {
            if (name.Equals(EventNames.MessagePublishing))
                OnMessagePublishing(payload);
            else if (name.Equals(EventNames.MessageReturned))
                OnMessageReturned(payload);
            else if (name.Equals(EventNames.MessageGot))
                OnMessageGot(payload);
            else if (name.Equals(EventNames.MessageReplied))
                OnMessageReplied(payload);
            else if (name.Equals(EventNames.MessageConsumed))
                OnMessageConsumed(payload);
        }

        private void OnMessagePublishing(object? payload)
        {
            var eventItems = EventItemBuilder.BuildFromMessagePublishing(payload, _options.DiagnosticsOptions);
            Log(eventItems, "Message publishing");
        }

        private void OnMessageReturned(object? payload)
        {
            var eventItems = EventItemBuilder.BuildFromMessageReturned(payload, _options.DiagnosticsOptions);
            Log(eventItems, "Message returned");
        }

        private void OnMessageGot(object? payload)
        {
            var eventItems = EventItemBuilder.BuildFromMessageGot(payload, _options.DiagnosticsOptions);
            Log(eventItems, "Message got");
        }

        private void OnMessageReplied(object? payload)
        {
            var eventItems = EventItemBuilder.BuildFromMessageReplied(payload, _options.DiagnosticsOptions);
            Log(eventItems, "Message replied");
        }

        private void OnMessageConsumed(object? payload)
        {
            var eventItems = EventItemBuilder.BuildFromMessageConsumed(payload, _options.DiagnosticsOptions);
            Log(eventItems, "Message consumed");
        }

        private void Log(EventItem[]? eventItems, string logPrefix)
        {
            if (eventItems is null)
                return;

            var messageBuilder = new StringBuilder($"{logPrefix}: ");
            var parameters = new List<object?>();
            foreach (var eventItem in eventItems)
            {
                var itemName = eventItem.Name.Replace('.', '_');
                messageBuilder.Append($"{eventItem.Description} = {{{itemName}}}; ");
                parameters.Add(eventItem.Value);
            }

            _logger.LogInformation(messageBuilder.ToString(), parameters.ToArray());
        }
    }
}