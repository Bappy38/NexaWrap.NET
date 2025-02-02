# NexaWrap.NET

NexaWrap.NET is a .NET solution that contains multiple wrapper projects for various AWS services and utility patterns. Each wrapper is implemented as a separate NuGet package, making it easy to integrate into different .NET applications.

## Projects

- **NexaWrap.SQS.NET**: A wrapper for Amazon SQS that simplifies message sending and receiving.

---
<br><br><br>

# NexaWrap.SQS.NET

## How to Configure SQS Queue?

1. **Create an IAM User**: Generate an IAM user with the necessary credentials to access SQS.
2. **Assign SQS Permissions**: Attach the appropriate policy to grant SQS access.
3. **Store Credentials**: Add `AWSAccessKey` and `AWSSecretKey` in your application configuration.
4. **Create a Queue**: Create an SQS queue and provide the IAM user ARN to grant access.
5. **Set AWS Region**: Specify the AWS region where the queue is created in the `SqsOptions` section of the config file.
6. **Microservices Recommendation**: Each microservice should have its own queue. For example:
   - `SkillWave.CourseManager-Dev`: Queue for `SkillWave.CourseManager` service.
   - `SkillWave.FileStore-Dev`: Queue for `SkillWave.FileStore` service.
   - Any service can publish messages to any queue, but each service should listen to only one queue.

---

## How to Use NexaWrap.SQS.NET in a .NET App?

### 1️⃣ Install the Package

```sh
// Install the SQS wrapper package
dotnet add package NexaWrap.SQS.NET
```

### 2️⃣ Create a Message

Implement the `IMessage` interface:

```csharp
public record CustomerCreated : IMessage
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string MessageTypeName => nameof(CustomerCreated);
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}
```

### 3️⃣ Create a Message Handler

Implement the `IMessageHandler<T>` interface:

```csharp
public class CustomerCreatedHandler : IMessageHandler<CustomerCreated>
{
    private readonly ILogger<CustomerCreatedHandler> _logger;

    public CustomerCreatedHandler(ILogger<CustomerCreatedHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(CustomerCreated message)
    {
        _logger.LogInformation("Customer created with name {Name} and CorrelationId {CorrelationId}", message.Name, message.CorrelationId);
        await Task.CompletedTask;
    }
}
```

### 4️⃣ Configure SQS in `appsettings.json`

```json
{
  "SqsOptions": {
    "SubscribedQueueName": "SkillWaveCourseManager-Dev",
    "AwsAccessKey": "your-access-key",
    "AwsSecretKey": "your-secret-key",
    "AwsRegion": "us-east-1"
  }
}
```

### 5️⃣ Register Handlers in `Program.cs`

```csharp
builder.Services.AddSqsMessageBus(builder.Configuration, builder =>
{
    builder.RegisterHandler<CustomerCreated, CustomerCreatedHandler>();
    builder.RegisterHandler<OrderCreated, OrderCreatedHandler>();
});
```

### 6️⃣ Example Controllers for Sending Messages

```csharp
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly IMessageSender _messageSender;
    
    public CustomerController(IMessageSender messageSender)
    {
        _messageSender = messageSender;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreated request)
    {
        request.CorrelationId = Guid.NewGuid().ToString();
        await _messageSender.SendAsync("SkillWaveCourseManager-Dev", request);
        return Ok(new { Message = "Customer Created", CorrelationId = request.CorrelationId });
    }
}
```

---