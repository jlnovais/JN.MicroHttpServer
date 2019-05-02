using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace JN.MicroHttpServer.WinServiceTest
{
    public partial class Service1 : ServiceBase
    {
        private readonly Timer _timer = new Timer {Interval = 60000};
        private readonly ILogWriter _logger;
        private IMicroHttpServer _httpServer;

        public Service1(ILogWriter logger, IMicroHttpServer httpServer)
        {
            _httpServer = httpServer;
            _logger = logger;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            _timer.Start();
            _logger.LogMessage("Service is starting. " + DateTime.Now);
        }

        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            _logger.LogMessage("Service is running. " + DateTime.Now);
        }

        protected override void OnStop()
        {
            _logger.LogMessage("Service is stopping. " + DateTime.Now);
            _timer.Stop();
        }

        public void StartService()
        {
            _httpServer.Start();
            OnStart(null);
        }

        public void StopService()
        {
            _httpServer.Stop();
            OnStop();
        }
    }
}
