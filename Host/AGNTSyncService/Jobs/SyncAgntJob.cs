using BOC.Services.LogService;
using BOC.SynchronousService.Config;
using BOC.SynchronousService.Framework.Common;
using BOC.SynchronousService.Framework.Config;
using Quartz;
using System;
using System.Configuration;
using System.Diagnostics;

namespace BOC.SynchronousService.Host.AGNTSyncService
{
    public class SyncAgntJob : IJob
    {
        internal static readonly string StartSync = "SyncAgntJob同步操作开始.";
        internal static readonly string EndSync = "SyncAgntJob同步操作结束.";
        
        private static readonly object LockObj = new object();
        public void Execute(IJobExecutionContext context)
        {
            lock (LockObj)
            {
                try
                {
                    EventLoggerHelper.WriteEventLogEntry
                      (
                          Utility.EventSource
                          , Utility.EventID
                          , StartSync
                          , Utility.Category
                          , EventLogEntryType.Information
                      );                    
                    var config = AppConfig.ConfigList[GetType().Name];
                    var service = new SyncAgntService();
                    service.Process(config);
                    EventLoggerHelper.WriteEventLogEntry
                      (
                          Utility.EventSource
                          , Utility.EventID
                          , EndSync
                          , Utility.Category
                          , EventLogEntryType.Information
                      );
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
}
