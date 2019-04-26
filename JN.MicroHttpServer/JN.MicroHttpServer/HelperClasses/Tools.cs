using System;
using System.Collections.Generic;
using System.Text;

namespace JN.MicroHttpServer.HelperClasses
{
    public class Tools
    {
        public static bool VerifyUrl(string rawUrl, string path)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
                return false;

            if (rawUrl.EndsWith("/" + path + "/") ||
                rawUrl.EndsWith("/" + path)
            )
                return true;

            return false;

        }
    }
}
