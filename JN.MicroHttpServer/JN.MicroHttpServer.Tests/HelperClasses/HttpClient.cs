using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using JN.MicroHttpServer.Entities;

namespace JN.MicroHttpServer.Tests.HelperClasses
{
    public static class HttpClient
    {
        public static async Task<(string, HttpStatusCode)> GetData(string url, string content, AccessDetails accessDetails, string method = "POST")
        {
            string contentText = "";
            HttpStatusCode statusCode;

            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
            {

                if (accessDetails != null)
                {
                    var authValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{accessDetails.Username}:{accessDetails.Password}")));

                    client.DefaultRequestHeaders.Authorization = authValue;
                }

                HttpResponseMessage response;

                switch (method)
                {
                    case "GET":
                        response = await client.GetAsync(url);
                        break;
                    case "POST":
                        var stringContent = new StringContent(content);
                        response = await client.PostAsync(url, stringContent);
                        break;
                    default:
                        throw new Exception("Method not supported");
                }

                statusCode = response.StatusCode;

                if (statusCode == HttpStatusCode.OK)
                    contentText = await response.Content.ReadAsStringAsync();

                response.Dispose();
            }

            return (contentText, statusCode);


        }
    }
}
