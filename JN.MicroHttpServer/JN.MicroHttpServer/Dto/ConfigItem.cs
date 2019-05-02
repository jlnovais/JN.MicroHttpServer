using System;

namespace JN.MicroHttpServer.Dto
{
    public class ConfigItem
    {
        public string Uri { get; set; }
        public HttpMethod HttpMethod { get; set; }
       
        /// <summary>
        /// AccessDetails = user details
        /// string = body content
        /// Result = output
        /// </summary>
        public Func<AccessDetails, string, Result> DelegateToExecute { get; set; }
    }
}
