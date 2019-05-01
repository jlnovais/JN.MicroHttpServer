using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JN.MicroHttpServer.Entities;

namespace JN.MicroHttpServer.HelperClasses
{
    public static class Tools
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


        public static ConfigItem GetConfigItem(this IEnumerable<ConfigItem> config, string url, string method)
        {
            ConfigItem item = null;
            try
            {
                item = config.FirstOrDefault(x => x.Uri == url && x.HttpMethod.ToString() == method);
            }
            catch (ArgumentNullException e)
            {
                Debug.WriteLine(e.Message);
            }
            
            return item;
        }

        public static bool ExistsUrlConfiguredWithOtherMethod(this IEnumerable<ConfigItem> config, string url, string method)
        {
            var res = false;
            try
            {
                res = config.Any(x => x.Uri == url && x.HttpMethod.ToString() != method);
            }
            catch (ArgumentNullException e)
            {
                Debug.WriteLine(e.Message);
            }

            return res;
        }

    }
}
