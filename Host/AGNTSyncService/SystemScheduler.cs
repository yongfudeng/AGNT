using BOC.SynchronousService.Framework.Common;
using BOC.SynchronousService.Framework.Config;
using Quartz;
using Quartz.Impl;
using System;
using System.Configuration;

namespace BOC.SynchronousService.Host.AGNTSyncService
{
    public class SystemScheduler
    {
        private IScheduler scheduler;
        public static readonly string AGNTSyncServiceNamespance = "BOC.SynchronousService.Host.AGNTSyncService";

        public static SystemScheduler CreateInstance()
        {
            return new SystemScheduler();
        }

        public IScheduler StdScheduler
        {
            get
            {
                if (scheduler == null)
                {
                    var sf = new StdSchedulerFactory();
                    scheduler = sf.GetScheduler();
                }
                return scheduler;
            }
        }
        public void StartScheduler()
        {
            scheduler.Start();
        }

        public void InitScheduler(ConfigBase config, string jobName)
        {
            var startTime = DateTimeOffset.Now;
            var jobGuid = Guid.NewGuid().ToString();

            #region 第一种 每天某个时间点 或时间段调用服务
            Type jobType = Type.GetType($"{AGNTSyncServiceNamespance}.{jobName}");
            var timeStr = config.ScheduleConfigDetail;
            if (config.ScheduleTriggerType == ScheduleType.CronTriggers)
            {
                var job = JobBuilder.Create(jobType).WithIdentity("job" + jobGuid, "group" + jobGuid).Build();
                ITrigger trigger = null;
#if DEBUG
                trigger = TriggerBuilder.Create()
                                 .WithIdentity("trigger" + jobGuid, "group" + jobGuid)
                                 .StartAt(startTime)
                                 .WithSimpleSchedule(x => x.WithIntervalInMinutes(60).WithRepeatCount(999999))
                                 .Build();
#else

            trigger = TriggerBuilder.Create()
                               .WithIdentity("trigger"+jobGuid, "group"+jobGuid)
                               .StartAt(startTime)
                               .WithCronSchedule(timeStr)
                               .Build();
#endif

                StdScheduler.ScheduleJob(job, trigger);
            }
            #endregion
            else
            {
                #region 第二种 每隔多长时间调用服务
                int time2;
                if (!int.TryParse(timeStr, out time2))
                {
                    time2 = 60;
                }
                var job2 = JobBuilder.Create(jobType).WithIdentity("job" + jobGuid, "group" + jobGuid).Build();
                var trigger2 = TriggerBuilder.Create()
                    .WithIdentity("trigger" + jobGuid, "group" + jobGuid)
                    .StartAt(startTime)
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(time2).WithRepeatCount(999999))
                    .Build();
                StdScheduler.ScheduleJob(job2, trigger2);
            }
            #endregion            
        }
        public void StopScheduler()
        {
            if (scheduler != null)
            {
                scheduler.Shutdown();
            }

        }
    }
}
