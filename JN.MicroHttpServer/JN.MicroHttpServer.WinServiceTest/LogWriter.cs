using NLog;
using System;

namespace JN.MicroHttpServer.WinServiceTest
{
    
    public interface ILogWriter
    {
        void LogMessage(string text);
        void LogErrorMessage(string text);

    }
    public class LogWriter: ILogWriter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void LogMessage(string text)
        {
            logger.Info(text);
            Console.WriteLine(text);
        }

        public void LogErrorMessage(string text)
        {
            logger.Error(text);
            Console.WriteLine(text);
        }
  
    }
}
