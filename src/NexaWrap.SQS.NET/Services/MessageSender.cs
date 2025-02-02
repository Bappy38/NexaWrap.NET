using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using NexaWrap.SQS.NET.Constants;
using NexaWrap.SQS.NET.Interfaces;
using NexaWrap.SQS.NET.Models;
using System.Text.Json;

namespace NexaWrap.SQS.NET.Services;

public class MessageSender : IMessageSender
{
    private readonly IAmazonSQS _sqsClient;
    private readonly ILogger<MessageSender> _logger;

    public MessageSender(
        IAmazonSQS sqsClient,
        ILogger<MessageSender> logger)
    {
        _sqsClient = sqsClient;
        _logger = logger;
    }

    public async Task SendMessageAsync<TMessage>(string queueName, TMessage message) where TMessage : IMessage
    {
        if (string.IsNullOrEmpty(message.CorrelationId))
        {
            message.CorrelationId = Guid.NewGuid().ToString();
        }

        var queueUrl = await _sqsClient.GetQueueUrlAsync(queueName);
        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl.QueueUrl,
            MessageBody = JsonSerializer.Serialize(message),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    MessageAttributes.MessageTypeName,
                    new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = message.MessageTypeName
                    }
                },
                {
                    MessageAttributes.CorrelationId,
                    new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = message.CorrelationId
                    }
                }
            }
        };
        await _sqsClient.SendMessageAsync(request);
        _logger.LogInformation("Message of type {MessageTypeName} sent successfully", message.MessageTypeName);
    }

    public async Task SendBatchMessageAsync<TMessage>(string queueName, List<TMessage> messages) where TMessage : IMessage
    {
        var queueUrl = await _sqsClient.GetQueueUrlAsync(queueName);

        foreach (var messageChunk in messages.Chunk(10))
        {
            var messageBatch = messageChunk.Select(m => new SendMessageBatchRequestEntry
            {
                Id = Guid.NewGuid().ToString(),
                MessageBody = JsonSerializer.Serialize(m),
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        MessageAttributes.MessageTypeName,
                        new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = m.MessageTypeName
                        }
                    },
                    {
                        MessageAttributes.CorrelationId,
                        new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = m.CorrelationId
                        }
                    }
                }
            });

            var sendMessageBatchRequest = new SendMessageBatchRequest
            {
                QueueUrl = queueUrl.QueueUrl,
                Entries = messageBatch.ToList()
            };

            var batchSendResponse = await _sqsClient.SendMessageBatchAsync(sendMessageBatchRequest);

            if (batchSendResponse.Failed.Count > 0)
            {
                _logger.LogError("Failed to send {FailedCount} messages from a batch", batchSendResponse.Failed.Count);
            }
            else
            {
                _logger.LogInformation("Succesfully sent a batch message of size {Count}", sendMessageBatchRequest.Entries.Count);
            }
        }
    }
}

