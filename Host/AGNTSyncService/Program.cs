using Topshelf;
using BOC.SynchronousService.Framework.Common;
using BOC.Services.LogService;
using System.Configuration;
using System;
using System.Diagnostics;

namespace BOC.SynchronousService.Host.AGNTSyncService
{
    class Program
    {
        static void Main()
        {
            EventLoggerHelper.TryCreateEventLogSource("AGNTSynchronousServiceLog", Utility.EventSource);
            EventLoggerHelper.EnabledMaxEventLogEntryTypeLevel = EventLogEntryType.Information;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            HostFactory.Run(x =>
            {
                x.Service<SystemService>(s =>
                {
                    s.ConstructUsing(name => new SystemService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();
                x.SetDescription(ConfigurationManager.AppSettings["ServiceDescription"]);
                x.SetDisplayName(ConfigurationManager.AppSettings["DisplayName"]);
                x.SetServiceName(ConfigurationManager.AppSettings["ServiceName"]);
            });
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string exceptionInfo = TraceHelper.GetExceptionInfo(e.ExceptionObject as Exception);
            EventLoggerHelper.WriteEventLogEntry
                  (
                      Utility.EventSource
                      , Utility.EventID
                      , exceptionInfo
                      , Utility.Category
                      , EventLogEntryType.Error
                  );
            Console.WriteLine(exceptionInfo);
            WriteInformationToConsole("\r\n{0}", exceptionInfo);
            Console.ReadLine();
        }

        private static void WriteInformationToConsole(string format, params object[] args)
        {
            string info;

            try
            {
                info = string.Format(format, args);
            }
            catch (Exception)
            {
                info = format;
            }

            Console.WriteLine("\b{0}", info);
            Console.Write(">");
        }
    }
}
