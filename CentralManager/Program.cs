using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralManager
{
    public class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "admin", Password = "123456", VirtualHost = "dev-host" };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var corrId = Guid.NewGuid().ToString();
                    var props = channel.CreateBasicProperties();
                    var replyQueueName = channel.QueueDeclare("spider_manager_callback").QueueName;

                    props.ReplyTo = replyQueueName;
                    props.CorrelationId = corrId;

                    channel.ExchangeDeclare(exchange: "spider_manager",
                                   type: "topic");

                    var consumer = new EventingBasicConsumer(channel);
                    channel.BasicConsume(queue: replyQueueName,
                                         autoAck: true,
                                         consumer: consumer);

                    consumer.Received += (model, ea) =>
                    {
                        var received = ea.Body;
                        var message = Encoding.UTF8.GetString(received);
                        Console.WriteLine(" [x] Received '{0}'", message);
                    };

                    while (true)
                    {
                        Console.WriteLine("Enter a message or press [enter] to leave.");

                        string line = Console.ReadLine();

                        if (string.IsNullOrEmpty(line))
                            break;

                        string routingKey = "spider";

                        var body = Encoding.UTF8.GetBytes(line);

                        channel.BasicPublish(exchange: "spider_manager",
                                routingKey: routingKey,
                                basicProperties: props,
                                body: body);
                       
                        Console.WriteLine(" [x] Sent '{0}':'{1}'", routingKey, line);
                    }
                }
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
