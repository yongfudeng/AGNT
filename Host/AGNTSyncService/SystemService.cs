using System;
using System.Threading;
using BOC.Services.LogService;
using BOC.SynchronousService.Framework.Common;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using BOC.SynchronousService.Framework.Config;
using BOC.SynchronousService.Framework.Runtime;
using System.Windows.Forms;
using BOC.SynchronousService.Config;

namespace BOC.SynchronousService.Host.AGNTSyncService
{
    public class SystemService
    {
        internal static readonly string StartMessage = "AGNT Synchronous Service 启动成功.";
        internal static readonly string StopMessage = "AGNT Synchronous Service 成功停止.";

        private SystemScheduler systemScheduler;
        Thread threadforwork;
        public void Start()
        {
            if (threadforwork == null)
            {
                threadforwork = new Thread(Work);
            }
            threadforwork.IsBackground = true;
            threadforwork.Start();
            EventLoggerHelper.WriteEventLogEntry
                   (
                       Utility.EventSource
                       , Utility.EventID
                       , StartMessage
                       , Utility.Category
                       , EventLogEntryType.Information
                   );
        }

        public void Stop()
        {
            if (systemScheduler != null)
            {
                systemScheduler.StopScheduler();
            }

            if (threadforwork != null)
            {
                if (threadforwork.ThreadState == System.Threading.ThreadState.Running)
                {
                    threadforwork.Abort();
                }
            }
            EventLoggerHelper.WriteEventLogEntry
                  (
                      Utility.EventSource
                      , Utility.EventID
                      , StopMessage
                      , Utility.Category
                      , EventLogEntryType.Warning
                  );
        }

        private void Work()
        {
            try
            {
                systemScheduler = SystemScheduler.CreateInstance();
                foreach (var key in AppConfig.ConfigList.Keys)
                    systemScheduler.InitScheduler(AppConfig.ConfigList[key], key);
                systemScheduler.StartScheduler();
            }
            catch (Exception ex)
            {
                EventLoggerHelper.WriteEventLogEntry
                   (
                       Utility.EventSource
                       , Utility.EventID
                       , ex.ToString()
                       , Utility.Category
                       , EventLogEntryType.Error
                   );
            }
        }

    }
}

