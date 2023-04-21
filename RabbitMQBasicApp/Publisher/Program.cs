using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

ConnectionFactory factory = new();
factory.Uri = new("amqps://oqbnnpzu:0eUJtipAduZZcdM1EPsuIthFIGpG-QZs@sparrow.rmq.cloudamqp.com/oqbnnpzu");

using IConnection connection = factory.CreateConnection();
using IModel channel = connection.CreateModel();

#region P2P (Point to Point) Design
string queueName = "example-p2p-queue";
// declare queue
channel.QueueDeclare(
    queue: queueName,
    durable: false,
    autoDelete: false,
    exclusive: false);

// get message from user input
Console.WriteLine("Enter message:");
var message = Console.ReadLine();

byte[] body = Encoding.UTF8.GetBytes(s: message);
channel.BasicPublish(
    exchange: string.Empty,
    routingKey: queueName,
    body: body);

Console.WriteLine($"Sent message: {message}");
Console.WriteLine("Press any key to exit.");
Console.ReadKey();
#endregion

#region Publish/Subscribe Design
string exchangeFanout = "exchange-fanout";
channel.ExchangeDeclare(exchange: exchangeFanout, type: ExchangeType.Fanout);

while (true)
{
    Console.WriteLine("Enter message (or exit to quit):");
    var fanoutMessage = Console.ReadLine();
    if (message == "exit")
    {
        break;
    }

    var fanoutBody = Encoding.UTF8.GetBytes(fanoutMessage);
    channel.BasicPublish(exchange: exchangeFanout,
                         routingKey: "",
                         basicProperties: null,
                         body: fanoutBody);

    Console.WriteLine(" [x] Sent {0}", fanoutMessage);
}
#endregion

#region Work/Queue Design Model
channel.QueueDeclare(queue: "task_queue",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

var messageWork = GetMessage(args);
var bodyWork = Encoding.UTF8.GetBytes(messageWork);

var properties = channel.CreateBasicProperties();
properties.Persistent = true;

channel.BasicPublish(exchange: "",
                     routingKey: "task_queue",
                     basicProperties: properties,
                     body: bodyWork);

Console.WriteLine(" [x] Sent {0}", messageWork);
Console.ReadLine();
static string GetMessage(string[] args)
{
    return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
}
#endregion

#region Request/Response Design Model​
string requestQueueName = "example-request-response-queue";
channel.QueueDeclare(
    queue: requestQueueName,
    durable: false,
    exclusive: false,
    autoDelete: false);

string replyQueueName = channel.QueueDeclare().QueueName;

string correlationId = Guid.NewGuid().ToString();

#region Creating and Sending Request Message
IBasicProperties propertiesReqRes = channel.CreateBasicProperties();
propertiesReqRes.CorrelationId = correlationId;
propertiesReqRes.ReplyTo = replyQueueName;

for (int i = 0; i < 10; i++)
{
    byte[] messageReqRes = Encoding.UTF8.GetBytes("merhaba" + i);
    channel.BasicPublish(
        exchange: string.Empty,
        routingKey: requestQueueName,
        body: messageReqRes,
        basicProperties: propertiesReqRes);
}
#endregion
#region Listening Response Queue
EventingBasicConsumer consumer = new(channel);
channel.BasicConsume(
    queue: replyQueueName,
    autoAck: true,
    consumer: consumer);

consumer.Received += (sender, e) =>
{
    if (e.BasicProperties.CorrelationId == correlationId)
    {

        Console.WriteLine($"Response : {Encoding.UTF8.GetString(e.Body.Span)}");
    }
};
#endregion

#endregion