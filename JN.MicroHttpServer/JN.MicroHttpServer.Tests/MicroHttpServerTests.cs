using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using NUnit.Framework;

namespace JN.MicroHttpServer.Tests
{
    public class MicroHttpServerTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
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