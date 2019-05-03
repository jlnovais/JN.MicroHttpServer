using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Timers;

namespace JN.MicroHttpServer.WinServiceTest
{
    public partial class TestService : ServiceBase
    {
        private readonly Timer _timer = new Timer {Interval = 60000};
        private readonly ILogWriter _logger;
        private IMicroHttpServer _httpServer;

        public TestService(ILogWriter logger, IMicroHttpServer httpServer)
        {
            _httpServer = httpServer;
            _logger = logger;
            InitializeComponent();
        }


        protected override void OnStart(string[] args)
        {
            var res = _httpServer.Start();

            if (!res.Success)
            {
                _logger.LogErrorMessage($"Error starting web server: {res.ErrorDescription}");
            }
            else
            {
                _logger.LogMessage("Web server started");
            }

            _logger.LogMessage("Service is starting... " + DateTime.Now);
            _timer.Elapsed += OnTimer;
            _timer.Start();
            _logger.LogMessage("Service is started " + DateTime.Now);

        }

        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            _logger.LogMessage("Service is running. " + DateTime.Now);
        }

        protected override void OnStop()
        {
            _logger.LogMessage("Service is stopping. " + DateTime.Now);

            _httpServer.Stop();

            _timer.Stop();
            _timer.Dispose();
        }

        public void StartService()
        {
            OnStart(null);
        }

        public void StopService()
        {
            OnStop();
            
        }
    }
}
