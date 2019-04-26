using System;
using System.Collections.Generic;
using System.Text;
using JN.MicroHttpServer.Entities;

namespace JN.MicroHttpServer
{
    public interface IMicroHttpServer
    {
        /// <summary>
        /// delegate for execute shutdown
        /// </summary>
        Func<string, string, Result> ExecuteShutdown { get; set; }

        /// <summary>
        /// delegate to get a JSON containing the service status
        /// </summary>
        Func<string> GetStatusHandler { get; set; }

        /// <summary>
        /// delegate to write debug messages
        /// </summary>
        Action<string> WriteOutputHandler { get; set; }

        /// <summary>
        /// list of base URI prefixes (ex: http://localhost:6000)
        /// </summary>

        /// <summary>
        /// delegate to validate a user
        /// </summary>
        Func<AccessDetails, bool> ValidateUser { get; set; }

        IEnumerable<string> UriPrefixes { get; set; }
        
        bool IsRunning { get; }
        Result Start();
        void Stop();

        bool IsInitialized { get; }


    }
}
