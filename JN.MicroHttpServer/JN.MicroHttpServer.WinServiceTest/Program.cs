using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        /// 
        private static TestService _serviceToRun;

        private static readonly ILogWriter LogWriter = new LogWriter();

        static void Main()
        {
            var server = GetServer();

            _serviceToRun = new TestService(LogWriter, server);


            if (Environment.UserInteractive)
            {
                Console.WriteLine($"Running as a Console Application");
                _serviceToRun.StartService();
                Console.WriteLine("Press any key to stop program...");
                Console.ReadLine();
                _serviceToRun.StopService();
                Console.WriteLine("Service Stopped.");
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    _serviceToRun,
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
                WriteOutputHandler = LogWriter.LogMessage,
                WriteOutputErrorHandler = LogWriter.LogErrorMessage,
                BasicAuthentication = true

            };

            return server;
        }



        private static Result Shutdown(AccessDetails details, string content, NameValueCollection queryString)
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

            LogWriter.LogMessage("Received request - Shutdown");

            _serviceToRun.StopService();
            Environment.Exit(0);

            return new Result()
            {
                Success = true
            };
        }


        private static Result GetStatus(AccessDetails details, string content, NameValueCollection queryString)
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

            LogWriter.LogMessage("Received request - GetStatus");

            return new Result()
            {
                Content = "info from service: I'm running.... Received items from query string:" + queryString.Count,
                Success = true
            };
        }

        private static bool ValidAccessDetails(AccessDetails details)
        {
            return details.Password == "123" && details.Username == "test";
        }
    }
}
