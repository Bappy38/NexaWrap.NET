using NexaWrap.SQS.NET.Models;

namespace NexaWrap.SQS.NET.Interfaces;

public interface IMessageSender
{
    Task SendMessageAsync<TMessage>(string queueName, TMessage message) where TMessage : IMessage;

    Task SendMessagesAsync<TMessage>(string queueName, List<TMessage> messages) where TMessage : IMessage;
}
