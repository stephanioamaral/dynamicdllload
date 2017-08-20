using Helper;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SpiderDefault;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;

namespace SpiderConsole
{
    public class SpiderService : ServiceBase
    {
        private Log logger = new Log("SpiderService");

        private readonly string serviceName = "Spider Service";
        private readonly string directory = "Spiders";

        private IConnection RabbitConnection;
        private IModel RabbitChannel;

        private List<ISpider<string>> spiders;

        public SpiderService()
        {
            ServiceName = serviceName;
            spiders = new List<ISpider<string>>();
        }

        public void Start(string[] args)
        {
            OnStart(args);
        }

        protected override void OnStart(string[] args)
        {
            logger.Info("Starting Spider Service");

            StartSpiders();
            CentralManagerConnection();
        }

        protected override void OnStop()
        {
            logger.Info("Stoping Spider Service");

            StopSpiders();

            RabbitConnection.Dispose();
            RabbitChannel.Dispose();
        }

        private List<Type> LoadDLL()
        {
            List<Type> types = new List<Type>();

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, directory);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            //foreach (string dll in Directory.GetFiles(path, "*.dll"))
            //{
            //    var assembly = Assembly.LoadFile(dll);
            //    var objects = assembly.GetTypes().Where(p => p.IsClass && !p.IsAbstract && typeof(ISpider<string>).IsAssignableFrom(p));
            //    types.AddRange(objects);
            //}

            foreach (string dll in Directory.GetFiles(path, "*.dll"))
            {
                using (FileStream fs = File.Open(dll, FileMode.Open))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] buffer = new byte[1024];
                        int read = 0;
                        while ((read = fs.Read(buffer, 0, 1024)) > 0)
                            ms.Write(buffer, 0, read);
                        var assembly = Assembly.Load(ms.ToArray());
                        var objects = assembly.GetTypes().Where(p => p.IsClass && !p.IsAbstract && typeof(ISpider<string>).IsAssignableFrom(p));
                        types.AddRange(objects);
                    }
                }
            }

            return types;
        }

        private void StartSpiders()
        {
            List<Type> types = LoadDLL();

            if (types.Count() == 0)
            {
                logger.Info("No spider is avaliable to run");
                return;
            }

            foreach (var item in types)
            {
                ISpider<string> instance = (ISpider<string>)Activator.CreateInstance(item);
                ISpider<string> spider = spiders.FirstOrDefault(p => p.ID == instance.ID);

                if (spider == null)
                    spiders.Add(instance);
                else
                {
                    if (!spider.Running)
                    {
                        spiders.RemoveAll(p => p.ID == spider.ID);
                        spiders.Add(instance);
                    }
                }
            }

            foreach (var item in spiders)
            {
                if (item.AutoStart)
                    item.Start();
            }
        }

        private void StopSpiders()
        {
            foreach (var item in spiders)
                item.Stop();
        }

        private void RestartSpiders()
        {
            foreach (var item in spiders)
                item.Restart();
        }

        private void AlterThreadNumerSpiders(string n)
        {
            int number = Convert.ToInt32(n);
            foreach (var item in spiders)
                item.ChangeThreadNumber(number);
        }

        private void AlterWaitSpiders(string n)
        {
            int number = Convert.ToInt32(n);
            foreach (var item in spiders)
                item.ChangeWait(number);
        }

        private void AlterNumberSpiders(string n)
        {
            int number = Convert.ToInt32(n);
            foreach (var item in spiders)
                item.ChangeMaxItemProcessing(number);
        }

        public void AlterModeSpiders(string n)
        {
            int number = Convert.ToInt32(n);
            SpiderMode mode = (SpiderMode)number;
            foreach (var item in spiders)
                item.ChangeMode(mode);
        }

        public List<string> GetStatusSpiders()
        {
            List<string> list = new List<string>();

            foreach (var item in spiders)
                list.Add(item.GetStatus());

            return list;
        }

        private void StartSpider(string id)
        {
            ISpider<string> spider = spiders.Where(p => p.ID == id).FirstOrDefault();
            if (spider != null)
                spider.Start();
        }

        private void StopSpider(string id)
        {
            ISpider<string> spider = spiders.Where(p => p.ID == id).FirstOrDefault();
            if (spider != null)
                spider.Stop();
        }

        private void RestartSpider(string id)
        {
            ISpider<string> spider = spiders.Where(p => p.ID == id).FirstOrDefault();
            if (spider != null)
                spider.Restart();
        }

        private void AlterThreadNumerSpider(string id, string n)
        {
            ISpider<string> spider = spiders.Where(p => p.ID == id).FirstOrDefault();
            if (spider != null)
            {
                int number = Convert.ToInt32(n);
                spider.ChangeThreadNumber(number);
            }
        }

        private void AlterWaitSpider(string id, string n)
        {
            ISpider<string> spider = spiders.Where(p => p.ID == id).FirstOrDefault();
            if (spider != null)
            {
                int number = Convert.ToInt32(n);
                spider.ChangeWait(number);
            }
        }

        private void AlterNumberSpider(string id, string n)
        {
            ISpider<string> spider = spiders.Where(p => p.ID == id).FirstOrDefault();
            if (spider != null)
            {
                int number = Convert.ToInt32(n);
                spider.ChangeMaxItemProcessing(number);
            }
        }

        public void AlterModeSpider(string id, string n)
        {
            ISpider<string> spider = spiders.Where(p => p.ID == id).FirstOrDefault();
            if (spider != null)
            {
                int number = Convert.ToInt32(n);
                SpiderMode mode = (SpiderMode)number;
                spider.ChangeMode(mode);
            }
        }

        private void CentralManagerConnection()
        {
            logger.Info("Starting Central Manager Connection");

            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "admin", Password = "123456", VirtualHost = "dev-host" };

            RabbitConnection = factory.CreateConnection();
            RabbitChannel = RabbitConnection.CreateModel();

            RabbitChannel.ExchangeDeclare(exchange: "spider_manager", type: "topic");
            var queueName = RabbitChannel.QueueDeclare().QueueName;

            RabbitChannel.QueueBind(queue: queueName,
                              exchange: "spider_manager",
                              routingKey: "spider");

            var consumer = new EventingBasicConsumer(RabbitChannel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;
                logger.Info($"Command Received '{routingKey}':'{message}'");
                ProcessedCommand(message);
            };

            RabbitChannel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);
        }

        private void ProcessedCommand(string command)
        {
            var split = command.Split(' ');

            try
            {
                if (split.Length > 0)
                {
                    if (split[0].ToUpper() == "STOP")
                    {
                        if (split[1].ToUpper() == "ALL")
                            StopSpiders();
                        else
                            StopSpider(split[1]);
                    }
                    else if (split[0].ToUpper() == "START")
                    {
                        if (split[1].ToUpper() == "ALL")
                            StartSpiders();
                        else
                            StartSpider(split[1]);
                    }
                    else if (split[0].ToUpper() == "RESTART")
                    {
                        if (split[1].ToUpper() == "ALL")
                            RestartSpiders();
                        else
                            RestartSpider(split[1]);
                    }
                    else if (split[0].ToUpper() == "THREAD")
                    {
                        if (split[1].ToUpper() == "ALL")
                            AlterThreadNumerSpiders(split[2]);
                        else
                            AlterThreadNumerSpider(split[1], split[2]);
                    }
                    else if (split[0].ToUpper() == "WAIT")
                    {
                        if (split[1].ToUpper() == "ALL")
                            AlterWaitSpiders(split[2]);
                        else
                            AlterWaitSpider(split[1], split[2]);
                    }
                    else if (split[0].ToUpper() == "NUMBER")
                    {
                        if (split[1].ToUpper() == "ALL")
                            AlterNumberSpiders(split[2]);
                        else
                            AlterNumberSpider(split[1], split[2]);
                    }
                    else if (split[0].ToUpper() == "MODE")
                    {
                        if (split[1].ToUpper() == "ALL")
                            AlterModeSpiders(split[2]);
                        else
                            AlterModeSpider(split[1], split[2]);
                    }
                    else if (split[0].ToUpper() == "RELOAD")
                    {
                        StartSpiders();
                    }
                    else if (split[0].ToUpper() == "STATUS")
                    {
                        List<string> list = GetStatusSpiders();
                    }
                    else
                    {
                        logger.Info($"Command not implemented: {command}");
                    }
                }
                else
                    logger.Info($"Command not valid: {command}");
            }
            catch (Exception)
            {
                logger.Error($"Command error: {command}");
            }
        }
    }
}
