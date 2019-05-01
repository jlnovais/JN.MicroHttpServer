using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JN.MicroHttpServer.Entities;
using JN.MicroHttpServer.HelperClasses;
using Newtonsoft.Json;

namespace JN.MicroHttpServer
{
    public interface IMicroHttpServer2
    {
        Action<string> WriteOutputHandler { get; set; }
        bool IsRunning { get; }
        bool IsInitialized { get; }
        Action<string> WriteOutputErrorHandler { get; set; }
        Result Start();
        void Stop();
    }

    public class MicroHttpServer2 : IMicroHttpServer2
    {

        private const string AllowedContentType = "application/json";

        private CancellationTokenSource _cts;
        private Task _t;
        private string _lastError = "";

        private readonly IEnumerable<ConfigItem> _config;


        public Action<string> WriteOutputHandler { get; set; }
        public Action<string> WriteOutputErrorHandler { get; set; }

        public bool IsRunning { get; private set; } = false;

        public bool BasicAuthentication { get; set; }


        private void WriteErrorOutput(string text)
        {
            WriteOutputHandler?.Invoke(text);
        }
        private void WriteOutput(string text)
        {
            WriteOutputHandler?.Invoke(text);
        }


        public MicroHttpServer2(IEnumerable<ConfigItem> config)
        {
            _config = config;
        }

        public Result Start()
        {
            _cts = new CancellationTokenSource();

            _t = StartListener(_cts.Token);

            var res = new Result
            {
                Success = IsRunning,
                ErrorDescription = _lastError,
                ErrorCode = IsRunning ? 0 : -1
            };

            return res;
        }

        public void Stop()
        {
            if (!IsRunning) 
                return;

            IsRunning = false;
            _cts.Cancel();
            _t?.Wait();
        }

        public bool IsInitialized => _config != null && _config.Any();

        private async Task StartListener(CancellationToken token)
        {
            WriteOutput("Starting listener... ");

            var listener = new HttpListener();

            if (BasicAuthentication)
            {
                listener.AuthenticationSchemes = AuthenticationSchemes.Basic;
            }

            IsRunning = false;
            _lastError = "";

            try
            {

                if (!IsInitialized)
                {
                    throw new ArgumentException("Invalid configuration.");
                }

                foreach (var configItem in _config)
                {
                    listener.Prefixes.Add(configItem.Uri);
                    WriteOutput($"URL: {configItem.Uri} METHOD: {configItem.HttpMethod}");
                }

                listener.Start();
                IsRunning = true;
            }
            catch (Exception e)
            {
                WriteErrorOutput("Error starting listener: " + e.Message);
                _lastError = e.Message;
                throw;
            }
            
            token.Register(() => listener.Abort());

            while (!token.IsCancellationRequested)
            {
                HttpListenerContext context;

                try
                {
                    context = await listener.GetContextAsync().ConfigureAwait(false);

                    HandleRequest(context); // Note that this is *not* awaited
                }
                catch(Exception exception)
                {
                    WriteErrorOutput($"Error processing request: {exception.Message}");
                }
            }
        }

        private AccessDetails GetAccessDetails(HttpListenerContext context)
        {
            if (context.User == null)
                return null;

            var identity = (HttpListenerBasicIdentity) context.User.Identity;

            return new AccessDetails
            {
                Username = identity.Name,
                Password = identity.Password
            };
        }



        private async Task HandleRequest(HttpListenerContext context)
        {
            // Handle the request, ideally in an asynchronous way.
            // Even if not asynchronous, though, this is still run 
            // on a different (thread pool) thread

            WriteOutput($"New request {context.Request.HttpMethod} for URL: {context.Request.Url.AbsoluteUri} | thread id: {Thread.CurrentThread.ManagedThreadId}");


            var item = _config.GetConfigItem(context.Request.Url.AbsoluteUri);

            if (item == null)
            {
                await ReturnError(context, "Not found", HttpStatusCode.NotFound);
                return;
            }

            if (item.HttpMethod.ToString() != context.Request.HttpMethod)
            {
                await ReturnError(context, "Not allowed", HttpStatusCode.MethodNotAllowed);
                return;
            }
            

            //if (context.Request.ContentType != AllowedContentType)
            //{
            //    await ReturnError(context, "Unsupported Media Type", HttpStatusCode.UnsupportedMediaType);
            //    return;
            //}

            try
            {
                var contents = await GetRequestContentsAsync(context.Request);

                var accessDetails = GetAccessDetails(context);

                var result = item.DelegateToExecute(accessDetails, contents);

                if (!result.Authenticated)
                    throw new AuthenticationException("User not authenticated");

                if (!result.Success)
                    throw new Exception(result.ErrorDescription);

                if (!string.IsNullOrWhiteSpace(result.JsonContent))
                {
                    byte[] data = Encoding.UTF8.GetBytes(result.JsonContent);
                    context.Response.ContentType = "application/json";

                    await context.Response.OutputStream.WriteAsync(data, 0, data.Length);
                }

                context.Response.OutputStream.Close();
            }
            catch (AuthenticationException e)
            {
                await ReturnError(context, e.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                await ReturnError(context, e.Message, HttpStatusCode.InternalServerError);
            }
        }


        private async Task ReturnError(HttpListenerContext context, string description, HttpStatusCode httpCode)
        {
            var jsonString = $"{{\"error\":\"{description}\"}}";

            WriteErrorOutput(jsonString);

            byte[] data = Encoding.UTF8.GetBytes(jsonString);
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = (int) httpCode;
            await context.Response.OutputStream.WriteAsync(data, 0, data.Length);
            context.Response.OutputStream.Close();
        }



        private async Task<string> GetRequestContentsAsync(HttpListenerRequest Request)
        {
            string documentContents;
            using (Stream receiveStream = Request.InputStream)
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    documentContents = await readStream.ReadToEndAsync();
                }
            }
            return documentContents;
        }

    }
}
