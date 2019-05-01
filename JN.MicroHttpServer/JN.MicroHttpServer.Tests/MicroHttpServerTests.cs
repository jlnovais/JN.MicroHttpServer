using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JN.MicroHttpServer.Entities;
using NUnit.Framework;

namespace JN.MicroHttpServer.Tests
{
    /// <summary>
    /// MicroHttpServerTests: run in admin mode or use netsh to grant permissions to all the URLs used
    /// </summary>
    public class MicroHttpServerTests
    {

        private static readonly HttpClient client = new HttpClient();

        private const string urlNotExist = "http://localhost:1234/testNotFound/";
        private const string url1 = "http://localhost:1234/test1/";
        private const string url2 = "http://localhost:1234/test2/";


        private int DelegateSuccessCallCount;

        private Result DelegateSuccess(AccessDetails arg1, string arg2)
        {
            DelegateSuccessCallCount++;
            Console.WriteLine(arg2);
            return new Result() { Success = true };
        }

        private int DelegateErrorCallCount;

        private Result DelegateError(AccessDetails arg1, string arg2)
        {
            DelegateErrorCallCount++;
            Console.WriteLine(arg2);
            return new Result() { Success = false };
        }


        [SetUp]
        public void Setup()
        {
            DelegateSuccessCallCount = 0;
        }

        [Test]
        public async Task MicroHttpServer_delegateIsCalled()
        {
            var server = GetServer();
            server.Start();

            var result = await GetData(url1, "test", null, "POST");

            server.Stop();

            Assert.AreEqual(1, DelegateSuccessCallCount);
        }

        [Test]
        public async Task MicroHttpServer_delegateIsCalledWithError()
        {
            var server = GetServer();
            server.Start();

            var result = await GetData(url2, "test", null, "GET");

            server.Stop();

            Assert.AreEqual(1, DelegateErrorCallCount);
        }

        [TestCase(url1, "GET")]
        [TestCase(url2, "POST")]
        public async Task MicroHttpServer_CallWithWrongMethod_returnsNotAllowed(string url, string wrongMethod)
        {
            var server = GetServer();
            server.Start();

            var result = await GetData(url, "test", null, wrongMethod);

            server.Stop();

            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, result.Item2);
        }

        [Test]
        public async Task MicroHttpServer_CallNotExistingUrl_returnsNotFound()
        {
            var server = GetServer();
            server.Start();

            var result = await GetData(urlNotExist, "test", null, "POST");

            server.Stop();

            Assert.AreEqual(HttpStatusCode.NotFound, result.Item2);
        }


        //-----------------------
        private IMicroHttpServer2 GetServer()
        {
            var config = new List<ConfigItem>()
            {
                new ConfigItem()
                {
                    DelegateToExecute = DelegateSuccess,
                    HttpMethod = HttpMethod.POST,
                    Uri = url1
                },
                new ConfigItem()
                {
                    DelegateToExecute = DelegateError,
                    HttpMethod = HttpMethod.GET,
                    Uri = url2
                }
            };

            var server = new MicroHttpServer2(config)
            {
                WriteOutputHandler = Console.WriteLine,
                WriteOutputErrorHandler = Console.WriteLine
            };

            return server;
        }


        public async Task<(string, HttpStatusCode)> GetData(string url, string content, AccessDetails accessDetails, string method = "POST")
        {
            string contentText = "";
            HttpStatusCode statusCode;

            using (HttpClient client = new HttpClient())
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

                if(statusCode == HttpStatusCode.OK)
                    contentText = await response.Content.ReadAsStringAsync();

                response.Dispose();
            }

            return (contentText, statusCode);


        }
    }
}