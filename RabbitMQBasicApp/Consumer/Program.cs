using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

ConnectionFactory factory = new();
factory.Uri = new("amqps://oqbnnpzu:0eUJtipAduZZcdM1EPsuIthFIGpG-QZs@sparrow.rmq.cloudamqp.com/oqbnnpzu");

using IConnection connection = factory.CreateConnection();
using IModel channel = connection.CreateModel();

#region P2P (Point to Point) Design
string queueName = "example-p2p-queue";
channel.QueueDeclare(
    queue: queueName,
    durable: false,
    autoDelete: false,
    exclusive: false);

EventingBasicConsumer consumer = new(channel);
channel.BasicConsume(
    queue: queueName,
    autoAck: true,
    consumer: consumer);

consumer.Received += (sender, e) =>
{
    Console.WriteLine(Encoding.UTF8.GetString(e.Body.Span.ToArray()));
};
#endregion

#region Publish/Subscribe Design
channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

var queueNameFanout = channel.QueueDeclare().QueueName;
channel.QueueBind(queue: queueNameFanout, exchange: "logs", routingKey: "");

Console.WriteLine(" [*] Waiting for logs.");

var consumerFanout = new EventingBasicConsumer(channel);
consumerFanout.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var messageFanout = Encoding.UTF8.GetString(body);
    Console.WriteLine(" [x] {0}", messageFanout);
};
channel.BasicConsume(queue: queueNameFanout, autoAck: true, consumer: consumerFanout);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
#endregion

#region Work/Queue Design Model
channel.QueueDeclare(queue: "work-queue",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

// Bu özellik, her bir işçiye sadece bir mesajın gönderilmesini sağlar.
// İşçi, bir mesajı işleyene kadar bir sonraki mesajı almaz.
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

Console.WriteLine(" [*] Waiting for messages.");

var consumerWork = new EventingBasicConsumer(channel);
consumerWork.Received += (model, ea) =>
{
    var bodyWork = ea.Body.ToArray();
    var messageWork = Encoding.UTF8.GetString(bodyWork);
    Console.WriteLine(" [x] Received {0}", messageWork);

    int dots = messageWork.Split('.').Length - 1;
    System.Threading.Thread.Sleep(dots * 1000);

    Console.WriteLine(" [x] Done");

    // Mesajın işlenmesinin başarıyla tamamlandığını doğrula (ack)
    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
};
channel.BasicConsume(queue: "task_queue",
                     autoAck: false,
                     consumer: consumerWork);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
#endregion

#region Request/Response Design Model​

string requestQueueName = "example-request-response-queue";
channel.QueueDeclare(
    queue: requestQueueName,
    durable: false,
    exclusive: false,
    autoDelete: false);

EventingBasicConsumer consumerReqRes = new(channel);
channel.BasicConsume(
    queue: requestQueueName,
    autoAck: true,
    consumer: consumerReqRes);

consumerReqRes.Received += (sender, e) =>
{
    string message = Encoding.UTF8.GetString(e.Body.Span);
    Console.WriteLine(message);

    byte[] responseMessage = Encoding.UTF8.GetBytes($"İşlem tamamlandı. : {message}");
    IBasicProperties propertiesReqRes = channel.CreateBasicProperties();
    propertiesReqRes.CorrelationId = e.BasicProperties.CorrelationId;
    channel.BasicPublish(
        exchange: string.Empty,
        routingKey: e.BasicProperties.ReplyTo,
        basicProperties: propertiesReqRes,
        body: responseMessage);
};

#endregion
