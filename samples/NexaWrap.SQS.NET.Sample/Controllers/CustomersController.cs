using Microsoft.AspNetCore.Mvc;
using NexaWrap.SQS.NET.Interfaces;
using NexaWrap.SQS.NET.Sample.Messages;

namespace NexaWrap.SQS.NET.Sample.Controllers;


[Route("api/[controller]")]
[ApiController]
public class CustomersController : ControllerBase
{
    private readonly IMessageSender _messageSender;

    public CustomersController(IMessageSender messageSender)
    {
        _messageSender = messageSender;
    }

    [HttpPost]
    public async Task<ActionResult> Create()
    {
        var customerNames = new List<string>
        {
            "Bappy",
            "Jabed"
        };

        var messages = customerNames.Select(c => new CustomerCreated
        {
            Id = Guid.NewGuid(),
            Name = c,
            CorrelationId = Guid.NewGuid().ToString()
        }).ToList();

        await _messageSender.SendMessagesAsync("ByteBox-FileStore-Dev", messages);
        return Ok();
    }
}
