using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using JN.MicroHttpServer.Entities;

namespace JN.MicroHttpServer.WinServiceTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ILogWriter logger = new Logger();
            IMicroHttpServer2 server = GetServer();

            var serviceToRun = new Service1(logger, server);


            if (Environment.UserInteractive)
            {
                Console.WriteLine($"Running as a Console Application");
                serviceToRun.StartService();
                Console.WriteLine("Press any key to stop program...");
                Console.ReadLine();
                serviceToRun.StopService();
                Console.WriteLine("Service Stopped.");
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    serviceToRun,
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        private static IMicroHttpServer2 GetServer()
        {

            var config = new List<ConfigItem>()
            {
                new ConfigItem()
                {
                    DelegateToExecute = Delegate1,
                    HttpMethod = HttpMethod.GET,
                    Uri = "http://localhost:9999/test/"

                }
            };


            var server = new MicroHttpServer2(config)
            {
                WriteOutputHandler = Console.WriteLine,
                WriteOutputErrorHandler = Console.WriteLine

            };

            return server;
        }
    

    private static Result Delegate1(AccessDetails arg1, string arg2)
        {
            Console.WriteLine($"Received request");

            return new Result() {Success = true};
        }
    }
}
