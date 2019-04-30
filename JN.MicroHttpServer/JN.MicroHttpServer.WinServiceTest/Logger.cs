using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JN.MicroHttpServer.WinServiceTest
{
    public interface ILogWriter
    {
        void LogMessage(string text);
        void LogErrorMessage(string text);
        void LogReceiveMessage(string text);
        void LogProcessedMessage(string text);
        void LogExpiredMessage(string text);
    }
    public class Logger: ILogWriter
    {
        public void LogMessage(string text)
        {
            Console.WriteLine(text);
        }

        public void LogErrorMessage(string text)
        {
            Console.WriteLine(text);
        }

        public void LogReceiveMessage(string text)
        {
            Console.WriteLine(text);
        }

        public void LogProcessedMessage(string text)
        {
            Console.WriteLine(text);
        }

        public void LogExpiredMessage(string text)
        {
            Console.WriteLine(text);
        }
    }
}
