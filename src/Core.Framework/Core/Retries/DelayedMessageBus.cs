using Xunit.Abstractions;
using Xunit.Sdk;

namespace Core.Framework.Core.Retries;

public class DelayedMessageBus : IMessageBus
{
    private readonly IMessageBus _innerBus;
    private readonly List<IMessageSinkMessage> _messages = new();

    public DelayedMessageBus(IMessageBus innerBus)
    {
        _innerBus = innerBus;
    }

    public bool QueueMessage(IMessageSinkMessage message)
    {
        lock (_messages)
        {
            _messages.Add(message);
        }
        return true;
    }

    public void Reset()
    {
        lock (_messages)
        {
            _messages.Clear();
        }
    }

    public void Dispose()
    {
        lock (_messages)
        {
            foreach (var message in _messages)
            {
                _innerBus.QueueMessage(message);
            }
            _messages.Clear();
        }
    }
}