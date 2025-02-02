using NexaWrap.SQS.NET.Interfaces;
using NexaWrap.SQS.NET.Models;

namespace NexaWrap.SQS.NET.Services;

public class MessageBusBuilder
{
    public Dictionary<Type, Type> HandlersDict { get; set; }

    public MessageBusBuilder()
    {
        HandlersDict = new Dictionary<Type, Type>();
    }

    public MessageBusBuilder RegisterHandler<TMessage, THandler>()
        where TMessage : class, IMessage
        where THandler : class, IMessageHandler<TMessage>
    {
        HandlersDict.Add(typeof(TMessage), typeof(THandler));
        return this;
    }
}
