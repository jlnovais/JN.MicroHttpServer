using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading;
using JN.MicroHttpServer.Entities;
using NUnit.Framework;

namespace JN.MicroHttpServer.Tests
{
    /// <summary>
    /// MicroHttpServerTests: run in admin mode or use netsh to grant permissions to all the URLs used
    /// </summary>
    public class MicroHttpServerTests
    {
        private string url1 = "http://localhost:12345/test1/";


        private int DelegateCallCount;

        private Result Delegate1(AccessDetails arg1, string arg2)
        {
            DelegateCallCount++;
            Console.WriteLine(arg2);
            return new Result() { Success = true };
        }


        [SetUp]
        public void Setup()
        {
            DelegateCallCount = 0;
        }

        [Test]
        public void Test1()
        {

            var server = GetServer();
            server.Start();


            Thread.Sleep(120000);

            var result = GetData(url1);

            server.Stop();

            Assert.Pass();
        }

        private MicroHttpServer2 GetServer()
        {
            var config = new List<ConfigItem>()
            {
                new ConfigItem()
                {
                    DelegateToExecute = Delegate1,
                    HttpMethod = HttpMethod.POST,
                    Uri = url1

                }
            };


            var server = new MicroHttpServer2(config)
            {
                WriteOutputHandler = Console.WriteLine
            };

            return server;
        }


        public string GetData(string url, string method = "POST")
        {
            using (WebClient client = new WebClient())
            {

                //client.Headers["User-Agent"] =
                //    "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0)";


                //// Download data.
                //byte[] arr = client. DownloadData("http://www.dotnetperls.com/");

                //// Write values.
                //Console.WriteLine("--- WebClient result ---");
                //Console.WriteLine(arr.Length);


                //var url = "https://your-url.tld/expecting-a-post.aspx";
                //var client = new WebClient();
                //var method = "POST"; // If your endpoint expects a GET then do it.
                var parameters = new NameValueCollection
                {
                    {"parameter1", "Hello world"},
                    {"parameter2", "www.stopbyte.com"},
                    {"parameter3", "parameter 3 value."}
                };


                /* Always returns a byte[] array data as a response. */
                var response_data = client.UploadValues(url, method, parameters);

                // Parse the returned data (if any) if needed.
                var responseString = UnicodeEncoding.UTF8.GetString(response_data);

                return responseString;

            }
        }
    }
}