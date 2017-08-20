using RabbitMQ.Client;
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
                    channel.ExchangeDeclare(exchange: "spider_manager",
                                   type: "topic");

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
                                basicProperties: null,
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
