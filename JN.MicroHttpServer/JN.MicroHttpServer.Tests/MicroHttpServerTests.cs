using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JN.MicroHttpServer.Dto;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine;
using NUnit.Framework;

namespace JN.MicroHttpServer.Tests
{
    /// <summary>
    /// MicroHttpServerTests: run in admin mode or use netsh to grant permissions to all the URLs used
    /// </summary>
    public class MicroHttpServerTests
    {

        private const string defaultErrorMessage = "something is wrong";
        private const int defaultErrorCode = -1;

        private const string urlNotExist = "http://localhost:1234/testNotFound/";
        private const string url1 = "http://localhost:1234/test1/";
        private const string url2 = "http://localhost:1234/test2/";
        private const string urlSuccessAuthenticated = "http://localhost:1234/test3/";
        private const string urlNotAuthenticated = "http://localhost:1234/test4/";
        private const string urlErrorAuthenticated = "http://localhost:1234/test5/";


        private int _delegateSuccessCallCount;
        private NameValueCollection _queryString;

        private Result DelegateSuccess(AccessDetails accessDetails, string content, NameValueCollection queryString)
        {
            _queryString = queryString;
            _delegateSuccessCallCount++;
            Console.WriteLine(content);
            return new Result() { Success = true };
        }

        private int _delegateErrorCallCount;

        private Result DelegateError(AccessDetails accessDetails, string content, NameValueCollection queryString)
        {
            _delegateErrorCallCount++;
            Console.WriteLine(content);
            return new Result()
            {
                ErrorDescription = defaultErrorMessage,
                ErrorCode = defaultErrorCode,
                Success = false
            };
        }

        private int _delegateSuccessAuthenticatedCallCount;
        private Result DelegateSuccessAuthenticated(AccessDetails accessDetails, string content, NameValueCollection queryString)
        {
            _delegateSuccessAuthenticatedCallCount++;
            Console.WriteLine(content);
            return new Result() { Success = true, Authenticated = true};
        }

        private int _delegateErrorAuthenticatedCallCount;
        private Result DelegateErrorAuthenticated(AccessDetails accessDetails, string content, NameValueCollection queryString)
        {
            _delegateErrorAuthenticatedCallCount++;
            Console.WriteLine(content);
            return new Result()
            {
                ErrorDescription = defaultErrorMessage,
                ErrorCode = defaultErrorCode,
                Success = false,
                Authenticated = true
            };
        }

        private int _delegateNotAuthenticatedCallCount;
        private Result DelegateNotAuthenticated(AccessDetails accessDetails, string content, NameValueCollection queryString)
        {
            _delegateNotAuthenticatedCallCount++;
            Console.WriteLine(content);
            return new Result() { Success = true, Authenticated = false };
        }



        [SetUp]
        public void Setup()
        {
            _queryString = null;
            _delegateNotAuthenticatedCallCount = 0;
            _delegateErrorAuthenticatedCallCount = 0;
            _delegateSuccessAuthenticatedCallCount = 0;
            _delegateErrorCallCount = 0;
            _delegateSuccessCallCount = 0;
        }


        [Test]
        public void MicroHttpServer_CanStartAndStopServer()
        {
            var server = GetServer();
            var res = server.Start();

            Assert.IsTrue(res.Success);
            Assert.IsTrue(server.IsInitialized);
            Assert.IsTrue(server.IsRunning);

            server.Stop();

            Assert.IsFalse(server.IsRunning);

        }


        [Test]
        public void MicroHttpServer_NoConfigation_returnsError()
        {
            var server = new MicroHttpServer(null);
            var res = server.Start();

            Assert.IsFalse(res.Success);

            server.Stop();
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
        public async Task MicroHttpServer_queryStringIsPassed()
        {
            var server = GetServer();
            server.Start();

            var result = await HelperClasses.HttpClient.GetData(url1+"?x=1&y=2", "test", null, "POST");

            server.Stop();

            Assert.AreEqual(2,_queryString.Count);
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



        [Test]
        public async Task MicroHttpServer_CalledWithError_returnsInternalServerError()
        {
            var server = GetServer();
            server.Start();

            var result = await HelperClasses.HttpClient.GetData(url2, "test", null, "GET");

            server.Stop();

            Assert.AreEqual(HttpStatusCode.InternalServerError, result.Item2);

            string expected = $"\"error\":\"{defaultErrorMessage}\"";
            string expected1 = $"\"errorCode\":\"{defaultErrorCode}\"";

            StringAssert.Contains(expected, result.Item1);
            StringAssert.Contains(expected1, result.Item1);

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
            var server = GetServer(true);
            server.Start();

            var result = await HelperClasses.HttpClient.GetData(url1, "test", GetAccessDetails(), "POST");

            server.Stop();

            Assert.AreEqual(1, _delegateSuccessCallCount);
        }


        [Test]
        public async Task MicroHttpServer_WithAuthentication_SuccessAuthenticated_returnsOK()
        {
            var server = GetServer(true);
            server.Start();

            var result = await HelperClasses.HttpClient.GetData(urlSuccessAuthenticated, "test", GetAccessDetails(), "GET");

            server.Stop();

            Assert.AreEqual(HttpStatusCode.OK, result.Item2);
        }

        [Test]
        public async Task MicroHttpServer_WithAuthentication_NotAuthenticated_returnsUnauthorized()
        {
            var server = GetServer(true);
            server.Start();

            var result = await HelperClasses.HttpClient.GetData(urlNotAuthenticated, "test", GetAccessDetails(), "GET");

            server.Stop();

            Assert.AreEqual(HttpStatusCode.Unauthorized, result.Item2);
        }

        [Test]
        public async Task MicroHttpServer_WithAuthentication_ErrorAuthenticated_returnsInternalServerError()
        {
            var server = GetServer(true);
            server.Start();

            var result = await HelperClasses.HttpClient.GetData(urlErrorAuthenticated, "test", GetAccessDetails(), "GET");

            server.Stop();

            Assert.AreEqual(HttpStatusCode.InternalServerError, result.Item2);

            string expected = $"\"error\":\"{defaultErrorMessage}\"";
            string expected1 = $"\"errorCode\":\"{defaultErrorCode}\"";

            StringAssert.Contains(expected, result.Item1);
            StringAssert.Contains(expected1, result.Item1);
        }
        //--------------------------

        private AccessDetails GetAccessDetails()
        {
            return new AccessDetails()
            {
                Username = "test",
                Password = "123"
            };
        }

        private IMicroHttpServer GetServer(bool useBasicAuthentication = false)
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
                },
                new ConfigItem()
                {
                    DelegateToExecute = DelegateSuccessAuthenticated,
                    HttpMethod = HttpMethod.GET,
                    Uri = urlSuccessAuthenticated
                },
                new ConfigItem()
                {
                    DelegateToExecute = DelegateNotAuthenticated,
                    HttpMethod = HttpMethod.GET,
                    Uri = urlNotAuthenticated
                },
                new ConfigItem()
                {
                    DelegateToExecute = DelegateErrorAuthenticated,
                    HttpMethod = HttpMethod.GET,
                    Uri = urlErrorAuthenticated
                }


            };

            var server = new MicroHttpServer(config)
            {
                WriteOutputHandler = Console.WriteLine,
                WriteOutputErrorHandler = Console.WriteLine,
                BasicAuthentication = useBasicAuthentication
            };

            return server;
        }


        
    }
}