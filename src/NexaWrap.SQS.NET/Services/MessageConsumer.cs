using Amazon.SQS.Model;
using Amazon.SQS;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexaWrap.SQS.NET.Models;
using System.Net;
using System.Text.Json;

namespace NexaWrap.SQS.NET.Services;

public class MessageConsumer : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly MessageDispatcher _messageDispatcher;
    private readonly ILogger<MessageConsumer> _logger;
    private readonly SqsOptions _sqsOptions;

    private readonly List<string> _messageAttributeNames = new List<string> { "All" };

    public MessageConsumer(
        IAmazonSQS sqsClient,
        MessageDispatcher messageDispatcher,
        ILogger<MessageConsumer> logger,
        IOptions<SqsOptions> sqsOptions)
    {
        _sqsClient = sqsClient;
        _messageDispatcher = messageDispatcher;
        _logger = logger;
        _sqsOptions = sqsOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueUrl = await _sqsClient.GetQueueUrlAsync(_sqsOptions.SubscribedQueueName, stoppingToken);

        var receiveRequest = new ReceiveMessageRequest
        {
            QueueUrl = queueUrl.QueueUrl,
            MessageAttributeNames = _messageAttributeNames,
            MessageSystemAttributeNames = _messageAttributeNames,
            MaxNumberOfMessages = _sqsOptions.MaxBatchSize,
            WaitTimeSeconds = _sqsOptions.WaitTimeSeconds
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            var messageResponse = await _sqsClient.ReceiveMessageAsync(receiveRequest, stoppingToken);

            if (messageResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Receive message request from SQS queue with name {SubscribedQueueName} failed with ResponseMetadata {ResponseMetadata}", _sqsOptions.SubscribedQueueName, messageResponse.ResponseMetadata);
                continue;
            }

            await ProcessMessagesAsync(messageResponse.Messages, queueUrl.QueueUrl, stoppingToken);
        }
    }

    private async Task ProcessMessagesAsync(List<Message> messages, string queueUrl, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();
        foreach (var message in messages)
        {
            tasks.Add(ProcessMessageAsync(message, queueUrl, cancellationToken));
        }
        await Task.WhenAll(tasks);
    }

    private async Task ProcessMessageAsync(Message message, string queueUrl, CancellationToken cancellationToken)
    {
        try
        {
            var messageBody = message.Body;
            var messageTypeName = message.MessageAttributes.GetValueOrDefault(nameof(IMessage.MessageTypeName))?.StringValue;

            if (string.IsNullOrEmpty(messageTypeName))
            {
                _logger.LogError("MessageConsumer received a message with null or empty MessageTypeName");
                return;
            }

            if (!_messageDispatcher.CanHandleMessageType(messageTypeName))
            {
                _logger.LogInformation("Didn't find any handler to handle the message with type name {messageTypeName}", messageTypeName);
                return;
            }

            var messageType = _messageDispatcher.GetMessageTypeByName(messageTypeName)!;
            var deserializedMessage = (IMessage)JsonSerializer.Deserialize(messageBody, messageType)!;

            await _messageDispatcher.DispatchAsync(deserializedMessage);

            _logger.LogInformation("Message with ID {MessageId}, CorrelationId {CorrelationId} processed successfully", message.MessageId, deserializedMessage.CorrelationId);

            await _sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }
}
