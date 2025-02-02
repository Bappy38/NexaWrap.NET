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
    public async Task<ActionResult> Create([FromBody] CustomerCreated customerCreated)
    {
        await _messageSender.SendMessageAsync("ByteBox-FileStore-Dev", customerCreated);
        return Ok();
    }
}
