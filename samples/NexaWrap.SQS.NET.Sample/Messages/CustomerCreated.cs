using NexaWrap.SQS.NET.Models;

namespace NexaWrap.SQS.NET.Sample.Messages;

public class CustomerCreated : IMessage
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public string MessageTypeName => nameof(CustomerCreated);

    public string? CorrelationId { get; set; }
}
