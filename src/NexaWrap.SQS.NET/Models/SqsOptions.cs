using System.ComponentModel.DataAnnotations;

namespace NexaWrap.SQS.NET.Models;

public class SqsOptions
{
    /// <summary>
    /// The name of the SQS queue to subscribe to.
    /// </summary>
    public string SubscribedQueueName { get; set; }

    /// <summary>
    /// The AWS access key ID for authentication.
    /// </summary>
    public string AwsAccessKey { get; set; }

    /// <summary>
    /// The AWS secret access key for authentication.
    /// </summary>
    public string AwsSecretKey { get; set; }

    /// <summary>
    /// The AWS region where the SQS queue is located.
    /// </summary>
    public string AwsRegion { get; set; }

    /// <summary>
    /// The maximum number of messages to receive in a single batch. Value must be within [1, 10].
    /// </summary>
    [Range(1, 10)]
    public int MaxBatchSize { get; set; } = 1;

    /// <summary>
    /// The number of seconds to wait for a message to arrive before returning an empty result. Value must be within [0, 20].
    /// </summary>
    [Range(0, 20)]
    public int WaitTimeSeconds { get; set; } = 20;
}