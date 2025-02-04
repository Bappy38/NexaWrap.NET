
# NexaWrap.SQS.NET

NexaWrap.SQS.NET is a lightweight, easy-to-use .NET wrapper for **Amazon Simple Queue Service (SQS)** that simplifies the process of sending and receiving messages in a structured way.  

## 🚀 Features
- **Seamless Message Sending** → Send messages to SQS queues with minimal code.  
- **Automatic Message Processing** → Receive and process messages asynchronously.  
- **Batch Processing Support** → Retrieve multiple messages in a single request.  
- **Built-in Error Handling** → Automatic retries and logging.  
- **Dependency Injection Friendly** → Easily integrate with ASP.NET Core applications.  

---

## 🔧 Configuration

To use **NexaWrap.SQS.NET**, add the following configuration in `appsettings.json`:

```json
{
  "SqsOptions": {
    "SubscribedQueueName": "queue-name",
    "AwsAccessKey": "your-access-key",
    "AwsSecretKey": "your-secret-key",
    "AwsRegion": "region",
    "MaxBatchSize": 1,
    "WaitTimeSeconds": 20
  }
}
```

### 🔹 **Configuration Properties**
| Property | Description |
|----------|------------|
| `SubscribedQueueName` | The name of the SQS queue the application will listen to. |
| `AwsAccessKey` | AWS access key for authentication. |
| `AwsSecretKey` | AWS secret key for authentication. |
| `AwsRegion` | AWS region where the SQS queue is created. |
| `MaxBatchSize` | Maximum number of messages received in one batch (default: `1`). |
| `WaitTimeSeconds` | Number of seconds to wait before returning an empty response when polling SQS (default: `20`). |

---

## 📦 Installation

To install NexaWrap.SQS.NET via NuGet:

```sh
dotnet add package NexaWrap.SQS.NET
```

---

## 📤 Sending Messages

Define a message by implementing the `IMessage` interface:

```csharp
public record CustomerCreated : IMessage
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string MessageTypeName => nameof(CustomerCreated);
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}
```

Inject `IMessageSender` and send a message:

```csharp
public class CustomerService
{
    private readonly IMessageSender _messageSender;

    public CustomerService(IMessageSender messageSender)
    {
        _messageSender = messageSender;
    }

    public async Task PublishCustomerCreatedEvent(int customerId, string name)
    {
        var message = new CustomerCreated
        {
            Id = customerId,
            Name = name
        };

        await _messageSender.SendAsync("queue-name", message);
    }
}
```

---

## 📥 Receiving & Handling Messages

Create a message handler by implementing `IMessageHandler<T>`:

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
        _logger.LogInformation("Processing customer: {Name} (ID: {Id})", message.Name, message.Id);
        await Task.CompletedTask;
    }
}
```

---

## 🛠️ Registering Dependencies in `Program.cs`

Register SQS services and message handlers in **ASP.NET Core**:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqsMessageBus(builder.Configuration, builder =>
{
    builder.RegisterHandler<CustomerCreated, CustomerCreatedHandler>();
});

var app = builder.Build();
app.Run();
```


