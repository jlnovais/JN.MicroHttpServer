using System;
using System.Collections.Generic;
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


        public static ConfigItem GetConfigItem(this IEnumerable<ConfigItem> config, string url)
        {
            ConfigItem item;
            try
            {
                item = config?.First(x => x.Uri == url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            return item;
        }

    }
}
