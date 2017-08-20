using System;
using System.ServiceProcess;

namespace SpiderConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            SpiderService service = new SpiderService();

            if (!Environment.UserInteractive)
            {
                // running as service
                ServiceBase.Run(service);
            }
            else
            {
                service.Start(args);

                Console.ReadKey(true);

                service.Stop();

                Console.ReadKey(true);
            }
        }
    }
}
