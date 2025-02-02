using NexaWrap.SQS.NET.Extensions;
using NexaWrap.SQS.NET.Sample.MessageHandlers;
using NexaWrap.SQS.NET.Sample.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureSqs(builder.Configuration, builder =>
{
    builder.RegisterHandler<CustomerCreated, CustomerCreatedHandler>();
});

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseHttpsRedirection();

app.Run();