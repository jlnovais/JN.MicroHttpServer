using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using JN.MicroHttpServer.Dto;
using JN.MicroHttpServer.HelperClasses;

namespace JN.MicroHttpServer
{
    public class MicroHttpServer : IMicroHttpServer
    {

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


        public MicroHttpServer(IEnumerable<ConfigItem> config)
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

                    HandleRequest(context); // don't await
                }
                catch (HttpListenerException) {  /*ignore*/ }
                catch (ObjectDisposedException) {  /*ignore*/ }
                catch (Exception exception)
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
            WriteOutput($"New {context.Request.HttpMethod} request for URL: {context.Request.Url.AbsoluteUri} | thread id: {Thread.CurrentThread.ManagedThreadId}");


            string urlPath = context.Request.Url.GetLeftPart(UriPartial.Path);
            string urlQuery = context.Request.Url.GetComponents(UriComponents.Query, UriFormat.UriEscaped);

            var queryString = HttpUtility.ParseQueryString(urlQuery);

            var item = _config.GetConfigItem(urlPath, context.Request.HttpMethod);

            if (_config.ExistsUrlConfiguredWithOtherMethod(urlPath, context.Request.HttpMethod))
            {
                await ReturnError(context, "Not allowed", (int)HttpStatusCode.MethodNotAllowed,  HttpStatusCode.MethodNotAllowed);
                return;
            }

            

            if (item == null)
            {
                await ReturnError(context, "Not found", (int)HttpStatusCode.NotFound, HttpStatusCode.NotFound);
                return;
            }


            var clientAcceptType = context.Request.AcceptTypes.GetAcceptedType();


            try
            {
                var contents = await GetRequestContentsAsync(context.Request);

                var accessDetails = GetAccessDetails(context);

                var result = item.DelegateToExecute(accessDetails, contents, queryString);

                if (!result.Authenticated)
                    throw new AuthenticationException("User not authenticated");

                if (!result.Success)
                    throw new ExecuteDelegateException(result.ErrorCode, result.ErrorDescription);

                if (!string.IsNullOrWhiteSpace(result.Content))
                {
                    byte[] data = Encoding.UTF8.GetBytes(result.Content);
                    context.Response.ContentType = clientAcceptType; //"application/json";

                    await context.Response.OutputStream.WriteAsync(data, 0, data.Length);
                }

                context.Response.OutputStream.Close();
            }
            catch (AuthenticationException e)
            {
                await ReturnError(context, e.Message, (int)HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized);
            }
            catch (ExecuteDelegateException e)
            {
                await ReturnError(context, e.ErrorDescription, e.StatusCode, HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                await ReturnError(context, e.Message, 0, HttpStatusCode.InternalServerError);
            }
        }


        private async Task ReturnError(HttpListenerContext context, string description, int errorCode, HttpStatusCode httpCode)
        {
            var jsonString = $"{{\"error\":\"{description}\",\"errorCode\":\"{errorCode}\"}}";

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
