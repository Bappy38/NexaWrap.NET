using NexaWrap.SQS.NET.Interfaces;
using NexaWrap.SQS.NET.Sample.Messages;

namespace NexaWrap.SQS.NET.Sample.MessageHandlers;

public class CustomerCreatedHandler : IMessageHandler<CustomerCreated>
{
    private readonly ILogger<CustomerCreatedHandler> _logger;

    public CustomerCreatedHandler(ILogger<CustomerCreatedHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(CustomerCreated message)
    {
        _logger.LogInformation("Customer created with name {Name}", message.Name);
        await Task.CompletedTask;
    }
}
