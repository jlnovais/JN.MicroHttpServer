using System;
using JN.MicroHttpServer.Dto;

namespace JN.MicroHttpServer
{
    public interface IMicroHttpServer
    {
        Action<string> WriteOutputHandler { get; set; }
        bool IsRunning { get; }
        bool IsInitialized { get; }
        Action<string> WriteOutputErrorHandler { get; set; }
        Result Start();
        void Stop();
    }
}