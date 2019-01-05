using System;
using System.IO;

namespace BOC.SynchronousService.Framework.Common
{
    public enum FileStag
    {
        DAT, TMP, RET,ERR
    }

    public enum ScheduleType
    {
        SimpleTrigger, CronTriggers
    }

    public static class Utility
    {
        public static readonly string EventSource = "AGNTSynchronousService";
        public static readonly int EventID = 9001;
        public static readonly short Category = 1;
        public static void WriteTestResult(string result, string path)
        {
            using (StreamWriter writer = File.AppendText(path))
            {
                writer.WriteLine(result);
            }
        }
    }
}