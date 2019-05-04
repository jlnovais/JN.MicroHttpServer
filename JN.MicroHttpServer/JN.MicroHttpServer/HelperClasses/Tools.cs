using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JN.MicroHttpServer.Dto;

namespace JN.MicroHttpServer.HelperClasses
{
    public static class Tools
    {

        public static string GetAcceptedType(this string[] types)
        {
            if (types == null)
                return "";

            var res = types.FirstOrDefault();

            return res ?? "";
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
