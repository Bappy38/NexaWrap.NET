using Amazon.Runtime;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NexaWrap.SQS.NET.Interfaces;
using NexaWrap.SQS.NET.Models;
using NexaWrap.SQS.NET.Services;

namespace NexaWrap.SQS.NET.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureSqs(this IServiceCollection services, IConfiguration configuration, Action<MessageBusBuilder> build)
    {
        var builder = new MessageBusBuilder();
        build.Invoke(builder);

        foreach (var (messageType, handlerType) in builder.HandlersDict)
        {
            services.AddScoped(handlerType);
        }

        services.AddSingleton<MessageDispatcher>(serviceProvider =>
        {
            var dispatcher = new MessageDispatcher(serviceProvider.GetRequiredService<IServiceScopeFactory>());

            foreach (var (messageType, handlerType) in builder.HandlersDict)
            {
                dispatcher.RegisterHandler(messageType, handlerType);
            }
            return dispatcher;
        });

        services.Configure<SqsOptions>(configuration.GetSection("SqsOptions"));

        services.AddScoped<IMessageSender, MessageSender>();

        services.AddHostedService<MessageConsumer>();

        services.AddSingleton<IAmazonSQS>(provider =>
        {
            var sqsOptions = provider.GetRequiredService<IOptions<SqsOptions>>().Value;
            var awsCredentials = new BasicAWSCredentials(sqsOptions.AwsAccessKey, sqsOptions.AwsSecretKey);
            var awsRegion = Amazon.RegionEndpoint.GetBySystemName(sqsOptions.AwsRegion);
            return new AmazonSQSClient(awsCredentials, awsRegion);
        });

        return services;
    }
}
