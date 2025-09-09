using System.Text;
using RabbitMQ.Client;

namespace DevHabit.Api.Services.MessageQueue;

public class RabbitMqService
{
    //private readonly ConnectionFactory _factory;
    //private readonly string _queueName;

    //public RabbitMqService(ConnectionFactory factory, string queueName)
    //{
    //    _factory = factory;
    //    _queueName = queueName;
    //}

    //public async Task SendMessageAsync(string message)
    //{
    //    await using var connection = await _factory.CreateConnectionAsync();
    //    await using var channel = await connection.CreateChannelAsync();

    //    await channel.QueueDeclareAsync(
    //        queue: _queueName,
    //        durable: false,
    //        exclusive: false,
    //        autoDelete: false,
    //        arguments: null
    //    );

        //var body = Encoding.UTF8.GetBytes(message);

        //await channel.BasicPublishAsync(
        //    exchange: "",
        //    routingKey: _queueName,
        //    mandatory: false,                 // novi parametar mandatory
        //    basicProperties: null,
        //    body: body.AsMemory()              // koristi ReadOnlyMemory<byte>
        //);
    //}
}
