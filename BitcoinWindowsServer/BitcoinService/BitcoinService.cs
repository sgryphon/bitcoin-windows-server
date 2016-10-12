using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinService
{
    public partial class BitcoinService : ServiceBase
    {
        private TraceSource trace = new TraceSource("BitcoinService");

        private Process bitcoindProcess;

        private string bitcoindPath;

        public string MainArgs
        {
            get;
            set;
        }

        public BitcoinService()
        {
            trace.TraceEvent(TraceEventType.Information, 1001, "BitcoinService Initialize");
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            trace.TraceEvent(TraceEventType.Information, 1100, "BitcoinService Starting");
            try
            {
                bitcoindPath = Environment.GetEnvironmentVariable("ProgramW6432");
                bitcoindPath += "\\Bitcoin\\daemon\\bitcoind.exe";
                trace.TraceEvent(TraceEventType.Verbose, 0, string.Format("Path: '{0}'", bitcoindPath));

                bitcoindProcess = new Process();
                bitcoindProcess.StartInfo = new ProcessStartInfo(bitcoindPath);
                string startArgs = string.Join(" ", args);
                trace.TraceEvent(TraceEventType.Verbose, 0, string.Format("StartArgs: '{0}'", startArgs));

                if (!string.IsNullOrEmpty(startArgs))
                {
                    trace.TraceEvent(TraceEventType.Verbose, 0, "Using startArgs");
                    bitcoindProcess.StartInfo.Arguments = startArgs;
                }
                else if (!string.IsNullOrEmpty(MainArgs))
                {
                    trace.TraceEvent(TraceEventType.Verbose, 0, "Using MainArgs");
                    bitcoindProcess.StartInfo.Arguments = MainArgs;
                }

                bitcoindProcess.ErrorDataReceived += new DataReceivedEventHandler(Bitcoind_ErrorDataReceived);
                bitcoindProcess.OutputDataReceived += new DataReceivedEventHandler(Bitcoind_OutputDataReceived);
                bitcoindProcess.Exited += new EventHandler(Bitcoind_Exited);
                bitcoindProcess.EnableRaisingEvents = true;

                bool started = bitcoindProcess.Start();
                trace.TraceEvent(TraceEventType.Verbose, 0, string.Format("Started: {0}", started));
            }
            catch (Exception ex)
            {
                trace.TraceEvent(TraceEventType.Error, 9100, string.Format("BitcoinService error starting: {0}", ex));
            }
        }

        private void Bitcoind_Exited(object sender, EventArgs eventArgs)
        {
            int exitCode = bitcoindProcess.ExitCode;
            trace.TraceEvent(TraceEventType.Verbose, 3, string.Format("EXITED: {0}", exitCode));
        }

        private void Bitcoind_OutputDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            trace.TraceEvent(TraceEventType.Verbose, 1, string.Format("OUT: {0}", eventArgs.Data));
        }

        private void Bitcoind_ErrorDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            trace.TraceEvent(TraceEventType.Warning, 2, string.Format("ERROR: {0}", eventArgs.Data));
        }

        protected override void OnStop()
        {
            trace.TraceEvent(TraceEventType.Information, 8100, "BitcoinService Stopping");
            try
            {
                // This no longer seems to work, due to security issues
                // Process.Start(bitcoindPath, "stop");
                bitcoindProcess.Kill();
                bool exited = bitcoindProcess.WaitForExit(60000);
                trace.TraceEvent(TraceEventType.Verbose, 0, string.Format("Bitcoin exit code: {0}", exited ? bitcoindProcess.ExitCode.ToString() : exited.ToString()));
            }
            catch (Exception arg)
            {
                trace.TraceEvent(TraceEventType.Error, 9101, string.Format("BitcoinService error stopping: {0}", arg));
            }
        }
    }
}
