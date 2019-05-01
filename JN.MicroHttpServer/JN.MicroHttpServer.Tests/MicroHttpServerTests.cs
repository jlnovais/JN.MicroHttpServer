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
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine;
using NUnit.Framework;

namespace JN.MicroHttpServer.Tests
{
    /// <summary>
    /// MicroHttpServerTests: run in admin mode or use netsh to grant permissions to all the URLs used
    /// </summary>
    public class MicroHttpServerTests
    {

        private const string urlNotExist = "http://localhost:1234/testNotFound/";
        private const string url1 = "http://localhost:1234/test1/";
        private const string url2 = "http://localhost:1234/test2/";


        private int _delegateSuccessCallCount;

        private Result DelegateSuccess(AccessDetails accessDetails, string content)
        {
            _delegateSuccessCallCount++;
            Console.WriteLine(content);
            return new Result() { Success = true };
        }

        private int _delegateErrorCallCount;

        private Result DelegateError(AccessDetails accessDetails, string content)
        {
            _delegateErrorCallCount++;
            Console.WriteLine(content);
            return new Result() { Success = false };
        }


        [SetUp]
        public void Setup()
        {
            _delegateSuccessCallCount = 0;
        }

        [Test]
        public async Task MicroHttpServer_delegateIsCalled()
        {
            var server = GetServer();
            server.Start();

            var result = await HelperClasses.HttpClient.GetData(url1, "test", null, "POST");

            server.Stop();

            Assert.AreEqual(1, _delegateSuccessCallCount);
        }

        [Test]
        public async Task MicroHttpServer_delegateIsCalledWithError()
        {
            var server = GetServer();
            server.Start();

            var result = await HelperClasses.HttpClient.GetData(url2, "test", null, "GET");

            server.Stop();

            Assert.AreEqual(1, _delegateErrorCallCount);
        }

        [TestCase(url1, "GET")]
        [TestCase(url2, "POST")]
        public async Task MicroHttpServer_CallWithWrongMethod_returnsNotAllowed(string url, string wrongMethod)
        {
            var server = GetServer();
            server.Start();

            var result = await HelperClasses.HttpClient.GetData(url, "test", null, wrongMethod);

            server.Stop();

            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, result.Item2);
        }

        [Test]
        public async Task MicroHttpServer_CallNotExistingUrl_returnsNotFound()
        {
            var server = GetServer();
            server.Start();

            var result = await HelperClasses.HttpClient.GetData(urlNotExist, "test", null, "POST");

            server.Stop();

            Assert.AreEqual(HttpStatusCode.NotFound, result.Item2);
        }

        [Test]
        public async Task MicroHttpServer_CallSuccess_returnsOk()
        {
            var server = GetServer();
            server.Start();

            var result = await HelperClasses.HttpClient.GetData(urlNotExist, "test", null, "POST");

            server.Stop();

            Assert.AreEqual(HttpStatusCode.NotFound, result.Item2);
        }

        //---------------------------
        [Test]
        public async Task MicroHttpServer_WithAuthentication_NoAuthHeaderInRequest_returnsUnauthorized()
        {
            var server = GetServer(true);
            server.Start();

            var result = await HelperClasses.HttpClient.GetData(url1, "test", null, "POST");

            server.Stop();
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.Item2);

        }

        [Test]
        public async Task MicroHttpServer_WithAuthentication_delegateIsCalled()
        {
            var server = GetServer();
            server.Start();

            var result = await HelperClasses.HttpClient.GetData(url1, "test", GetAccessDetails(), "POST");

            server.Stop();

            Assert.AreEqual(1, _delegateSuccessCallCount);
        }

        private AccessDetails GetAccessDetails()
        {
            return new AccessDetails()
            {
                Username = "test",
                Password = "123"
            };
        }

        //--------------------------
        private IMicroHttpServer2 GetServer(bool useBasicAuthentication = false)
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
                WriteOutputErrorHandler = Console.WriteLine,
                BasicAuthentication = useBasicAuthentication
            };

            return server;
        }


        
    }
}