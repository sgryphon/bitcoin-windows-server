using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinService
{
    static class Program
    {
        private static TraceSource trace = new TraceSource("BitcoinService");

        static void Main(string[] args)
        {
            bool hasArgs = args.Length != 0;
            if (hasArgs)
            {
                var arg0 = args[0].ToLowerInvariant();

                bool help = arg0.Equals("/help") || arg0.Equals("-help") || arg0.Equals("help")
                    || arg0.Equals("/?") || arg0.Equals("-?") || arg0.Equals("?");
                if (help)
                {
                    Console.WriteLine("Bitcoin Windows Service");
                    Console.WriteLine("");
                    Console.WriteLine(".\\BitcoinService.exe /install [\"service_args\"]");
                    Console.WriteLine("  Installs the Windows Service 'bitcoind', to run with the given arguments");
                    Console.WriteLine("  e.g. .\\BitcoinService.exe /install \"-datadir=F:\\Bitcoin\"");
                    Console.WriteLine("");
                    Console.WriteLine(".\\BitcoinService.exe /remove");
                    Console.WriteLine("  Removes the Windows Service");
                    return; 
                }

                bool install = arg0.Equals("/install") || arg0.Equals("-install") || arg0.Equals("install");
                if (install)
                {
                    trace.TraceEvent(TraceEventType.Information, 1000, "BitcoinService Install");
                    string mainArgsSuffix = string.Empty;
                    bool hasMainArgs = args.Length > 1;
                    if (hasMainArgs)
                    {
                        mainArgsSuffix = " " + args[1];
                    }
                    Process.Start("sc.exe", "create bitcoind binPath= \"" + Assembly.GetExecutingAssembly().Location + mainArgsSuffix + "\" start= auto obj= \"NT AUTHORITY\\Local Service\" password= \"\" DisplayName= \"Bitcoin Service\"").WaitForExit();
                    Process.Start("sc.exe", "config bitcoind start= delayed-auto").WaitForExit();
                    Console.WriteLine("Service Created");
                    Console.WriteLine("Copy bitcoin.conf to the bitcoin datadir");
                    Console.WriteLine("Default is C:\\Windows\\ServiceProfiles\\LocalService\\AppData\\Roaming\\Bitcoin");
                    return;
                }

                bool remove = arg0.Equals("/remove") || arg0.Equals("-remove") || arg0.Equals("remove");
                if (remove)
                {
                    trace.TraceEvent(TraceEventType.Information, 8000, "BitcoinService Remove");
                    Process.Start("sc.exe", "stop bitcoind").WaitForExit();
                    Process.Start("sc.exe", "delete bitcoind").WaitForExit();
                    Console.WriteLine("Service Deleted");
                    return;
                }
            }

            trace.TraceEvent(TraceEventType.Information, 1002, "BitcoinService Main");
            string mainArgs = string.Join(" ", args);
            trace.TraceEvent(TraceEventType.Verbose, 0, string.Format("MainArgs: '{0}'", mainArgs));
            var servicesToRun = new ServiceBase[]
            {
                new BitcoinService() {
                    MainArgs = mainArgs
                }
            };
            ServiceBase.Run(servicesToRun);
            trace.TraceEvent(TraceEventType.Verbose, 0, "Exit Main");
        }
    }
}
