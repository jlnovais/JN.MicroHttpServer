using System;

namespace JN.MicroHttpServer.HelperClasses
{
    public class ExecuteDelegateException : Exception
    {
        public int StatusCode { get; }
        public string ErrorDescription { get; }


        public ExecuteDelegateException(int statusCode, string errorDescription) : base(errorDescription)
        {
            StatusCode = statusCode;
            ErrorDescription = errorDescription;
        }

        public ExecuteDelegateException(int statusCode, string errorDescription, Exception inner)
            : base(errorDescription, inner)
        {
            StatusCode = statusCode;
            ErrorDescription = errorDescription;
        }
    }
}
