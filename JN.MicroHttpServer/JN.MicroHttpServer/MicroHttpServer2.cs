using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JN.MicroHttpServer.Entities;
using JN.MicroHttpServer.HelperClasses;
using Newtonsoft.Json;

namespace JN.MicroHttpServer
{
    public class MicroHttpServer2
    {

        private CancellationTokenSource _cts;
        private Task _t;
        private string _lastError = "";

        public Func<string, string, Result> ExecuteShutdown { get; set; }
        public Func<string> GetStatusHandler { get; set; }
        public Action<string> WriteOutputHandler { get; set; }
        public Func<AccessDetails, bool> ValidateUser { get; set; }

        public IEnumerable<ConfigItem> Config { get; set; }

        public bool IsRunning { get; private set; } = false;

        private void WriteOutput(string text)
        {
            WriteOutputHandler?.Invoke(text);
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
            //t?.Wait();
        }

        public bool IsInitialized => Config != null && Config.Any();

        private async Task StartListener(CancellationToken token)
        {
            WriteOutput("Starting listener... ");

            var listener = new HttpListener();

            IsRunning = false;
            _lastError = "";

            try
            {

                if (!IsInitialized)
                {
                    throw new ArgumentException("Invalid configuration.");
                }

                foreach (var configItem in Config)
                {
                    listener.Prefixes.Add(configItem.Uri);
                    WriteOutput($"URL: {configItem.Uri} METHOD: {configItem.HttpMethod}");
                }

                listener.Start();
                IsRunning = true;
            }
            catch (Exception e)
            {
                WriteOutput("Error starting listener: " + e.Message);
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
                catch
                {
                    // Handle errors
                }
            }
        }





        private async Task HandleRequest(HttpListenerContext context)
        {
            // Handle the request, ideally in an asynchronous way.
            // Even if not asynchronous, though, this is still run 
            // on a different (thread pool) thread

            WriteOutput($"New request {context.Request.HttpMethod} for URL: {context.Request.RawUrl} | thread id: {Thread.CurrentThread.ManagedThreadId}");

            if (context.Request.HttpMethod == "GET"  &&  Tools.VerifyUrl(context.Request.RawUrl, "status"))
            {
                try
                {
                    var jsonString = GetStatusHandler();

                    byte[] data = Encoding.UTF8.GetBytes(jsonString);
                    context.Response.ContentType = "application/json";

                    await context.Response.OutputStream.WriteAsync(data, 0, data.Length);
                    context.Response.OutputStream.Close();
                }
                catch (Exception e)
                {
                    await ReturnError(context, e.Message, HttpStatusCode.InternalServerError);
                }
                
            }
            else if (context.Request.HttpMethod == "POST" && Tools.VerifyUrl(context.Request.RawUrl, "shutdown"))
            {
                try
                {
                    var content = GetRequestContents(context.Request);

                    AccessDetails accessDetails = JsonConvert.DeserializeObject<AccessDetails>(content);

                    var resValidation = ValidateUser(accessDetails);
                    if (!resValidation)
                    {
                        await ReturnError(context, "Invalid access details", HttpStatusCode.Unauthorized);
                        return;
                    }

                    var res = ExecuteShutdown(accessDetails.Username, accessDetails.Password);

                    if (!res.Success)
                    {
                        string jsonString = JsonConvert.SerializeObject(res);
                        byte[] data = Encoding.UTF8.GetBytes(jsonString);
                        context.Response.ContentType = "application/json";

                        await context.Response.OutputStream.WriteAsync(data, 0, data.Length);
                    }

                    context.Response.OutputStream.Close();
                }
                catch (Exception e)
                {
                    await ReturnError(context, e.Message, HttpStatusCode.InternalServerError);

                    return;
                }
            }
            else
            {
                await ReturnError(context, "Invalid request", HttpStatusCode.InternalServerError);
            }
        }

        private async Task ReturnError(HttpListenerContext context, string description, HttpStatusCode httpCode)
        {
            var jsonString = $"{{\"error\":\"{description}\"}}";


            WriteOutput(jsonString);

            byte[] data = Encoding.UTF8.GetBytes(jsonString);
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = (int) httpCode;
            await context.Response.OutputStream.WriteAsync(data, 0, data.Length);
            context.Response.OutputStream.Close();
        }



        private string GetRequestContents(HttpListenerRequest Request)
        {
            string documentContents;
            using (Stream receiveStream = Request.InputStream)
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    documentContents = readStream.ReadToEnd();
                }
            }
            return documentContents;
        }

    }
}
