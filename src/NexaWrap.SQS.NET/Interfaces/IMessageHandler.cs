namespace NexaWrap.SQS.NET.Interfaces;

public interface IMessageHandler<TMessage>
{
    Task HandleAsync(TMessage message);
}
