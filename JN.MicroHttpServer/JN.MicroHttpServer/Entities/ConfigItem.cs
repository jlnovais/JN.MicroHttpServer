using System;
using System.Collections.Generic;
using System.Text;

namespace JN.MicroHttpServer.Entities
{
    public class ConfigItem
    {
        public string Uri { get; set; }
        public HttpMethod HttpMethod { get; set; }
        public string RequiresAuthentication { get; set; }
        /// <summary>
        /// AccessDetails = user details
        /// string = body content
        /// Result = output
        /// </summary>
        public Func<AccessDetails, string, Result> DelegateToExecute { get; set; }
    }
}
