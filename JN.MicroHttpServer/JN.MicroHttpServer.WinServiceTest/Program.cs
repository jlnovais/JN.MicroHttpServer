using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using JN.MicroHttpServer.Dto;

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
            IMicroHttpServer server = GetServer();

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

        private static IMicroHttpServer GetServer()
        {

            var config = new List<ConfigItem>()
            {
                new ConfigItem()
                {
                    DelegateToExecute = GetStatus,
                    HttpMethod = HttpMethod.GET,
                    Uri = "http://localhost:9999/status/"

                },

                new ConfigItem()
                {
                    DelegateToExecute = Shutdown,
                    HttpMethod = HttpMethod.GET,
                    Uri = "http://localhost:9999/exit/"

                }
            };


            var server = new MicroHttpServer(config)
            {
                WriteOutputHandler = Console.WriteLine,
                WriteOutputErrorHandler = Console.WriteLine,
                BasicAuthentication = true

            };

            return server;
        }

        private static Result Shutdown(AccessDetails details, string content)
        {
            if (details != null)
            {
                if (!ValidAccessDetails(details))
                {
                    return new Result()
                    {
                        Authenticated = false
                    };
                }
            }

            Environment.Exit(0);

            return new Result()
            {
                Success = true
            };
        }


        private static Result GetStatus(AccessDetails details, string content)
        {
            if (details != null)
            {
                if (!ValidAccessDetails(details))
                {
                    return new Result()
                    {
                        Success = false,
                        Authenticated = false
                    };
                }
            }

            Console.WriteLine($"Received request");

            return new Result()
            {
                Content = "info from service: I'm running....",
                Success = true
            };
        }

        private static bool ValidAccessDetails(AccessDetails details)
        {
            return details.Password == "123" && details.Username == "test";
        }
    }
}
