using System.Text.Json.Serialization;

namespace NexaWrap.SQS.NET.Models;

public interface IMessage
{
    [JsonIgnore]
    public string MessageTypeName { get; }
    [JsonIgnore]
    public string? CorrelationId { get; set; }
}