using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "hello",
                     durable: true, // marca a fila duravel não será apagada quando o rabbitmq for reiniciado
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);


/*
 Isso diz ao RabbitMQ para não fornecer mais de uma mensagem a um trabalhador por vez.
 Ou, em outras palavras, não envie uma nova mensagem para um trabalhador até que ele tenha processado e confirmado a anterior.
 Em vez disso, ele irá despachá-lo para o próximo trabalhador que ainda não esteja ocupado.
*/
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

Console.WriteLine(" [*] Waiting for messages.");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($" [x] Received {message}");

    int dots = message.Split('.').Length - 1;

    Thread.Sleep(dots * 1000);

    Console.WriteLine(" [x] Done");

    // here channel could also be accessed as ((EventingBasicConsumer)sender).Model
    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
};
channel.BasicConsume(queue: "hello",
                     autoAck: false,
                     consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();