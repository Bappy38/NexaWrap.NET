using Microsoft.Extensions.DependencyInjection;
using NexaWrap.SQS.NET.Interfaces;
using NexaWrap.SQS.NET.Models;

namespace NexaWrap.SQS.NET.Services;

public class MessageDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly Dictionary<string, Type> _messageTypeMappings = new();
    private readonly Dictionary<string, Type> _handlerTypeMappings = new();

    public MessageDispatcher(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    internal void RegisterHandler(Type messageType, Type handlerType)
    {
        var messageTypeName = messageType.Name;

        _messageTypeMappings.Add(messageTypeName, messageType);
        _handlerTypeMappings.Add(messageTypeName, handlerType);
    }

    public async Task DispatchAsync(object message)
    {
        var messageType = message.GetType();
        var messageTypeName = messageType.Name;
        _handlerTypeMappings.TryGetValue(messageTypeName, out var handlerType);

        if (handlerType == null)
        {
            throw new Exception("Handler type not found");
        }

        using var scope = _scopeFactory.CreateScope();

        var handler = scope.ServiceProvider.GetService(handlerType);

        if (handler is null)
        {
            throw new Exception($"Handler for message type {messageTypeName} not found");
        }

        var handleMethodName = nameof(IMessageHandler<IMessage>.HandleAsync);

        var handleMethod = handlerType.GetMethod(handleMethodName);

        if (handleMethod is null)
        {
            throw new Exception($"Handle method {handleMethodName} not found");
        }

        await (Task)handleMethod.Invoke(handler, [message]);
    }

    public bool CanHandleMessageType(string messageTypeName)
    {
        return _handlerTypeMappings.ContainsKey(messageTypeName);
    }

    public Type? GetMessageTypeByName(string messageTypeName)
    {
        return _messageTypeMappings.GetValueOrDefault(messageTypeName);
    }
}

