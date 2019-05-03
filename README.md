# JN.MicroHttpServer
Micro Http Server - small http server to be used with other applications such as windows services.

## Install
Download the package from NuGet:

`Install-Package JN.MicroHttpServer`

## Usage
You must create the a configuration containing `ConfigItem` objects. For each object a `DelegateToExecute` must be defined to process the requests received for that URL.

The server can be started using `Start` and stopped  with `Stop`.

Example:

```csharp

        private IMicroHttpServer GetServer()
        {
            var config = new List<ConfigItem>()
            {
                new ConfigItem()
                {
                    DelegateToExecute = ProcessRequest,
                    HttpMethod = HttpMethod.POST,
                    Uri = "http://localhost:1234/test1/"
                },
                new ConfigItem()
                {
                    DelegateToExecute = ProcessRequest,
                    HttpMethod = HttpMethod.GET,
                    Uri = "http://localhost:1234/test2/"
                }
            };

            var server = new MicroHttpServer(config)
            {
                WriteOutputHandler = Console.WriteLine,
                WriteOutputErrorHandler = Console.WriteLine,
                BasicAuthentication = false
            };

            return server;
        }
		
        private Result ProcessRequest(AccessDetails accessDetails, string content, NameValueCollection queryString)
        {
            //do stuff
			
			var resultProcessing = DoStuff();
			
            var result = new Result
			{
				Success = true
			};
			
			if(!resultProcessing)
			{
				result.ErrorDescription = "Error Description";
				result.ErrorCode = -1;
				result.Success = false;
			}
			
            return result;
        }
		
		...
		
		var server = GetServer();
		server.Start();
		...
		server.Stop();
```


