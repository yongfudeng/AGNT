using System;
using System.Diagnostics;
using System.Text;

namespace BOC.SynchronousService.Framework.Common
{
    public static class TraceHelper
    {
        private static BooleanSwitch boolSwitch = new BooleanSwitch("switch", "Switch in config file");

        public static void TraceInformation(string message)
        {
            if (boolSwitch.Enabled)
            {
                StringBuilder builder = new StringBuilder();

                builder.AppendFormat("\r\n{0}\r\n{1}\r\n\r\n",
                    "----------------------------------------------------",
                    DateTime.Now.ToString());

                builder.AppendFormat("{0}\r\n\r\n", message);

                builder.AppendFormat("{0}\r\n{1}\r\n\r\n",
                    DateTime.Now.ToString(),
                    "----------------------------------------------------");

                Trace.TraceInformation(builder.ToString());
            }
        }

        public static void TraceError(string message)
        {
            if (boolSwitch.Enabled)
            {
                StringBuilder builder = new StringBuilder();

                builder.AppendFormat("\r\n{0}\r\n{1}\r\n\r\n",
                    "----------------------------------------------",
                    DateTime.Now.ToString());

                builder.AppendFormat("{0}\r\n\r\n", message);

                builder.AppendFormat("{0}\r\n{1}\r\n\r\n",
                    DateTime.Now.ToString(),
                    "----------------------------------------------");

                Trace.TraceError(builder.ToString());
            }
        }

        public static string GetExceptionInfo(Exception ex)
        {
            StringBuilder strBuilder = new StringBuilder();

            for (Exception curException = ex; curException != null; curException = curException.InnerException)
            {
                strBuilder.AppendFormat("\r\n{0}\r\n", curException.Message);
                strBuilder.AppendFormat("¶ÑÕ»Îª:\r\n{0}\r\n", curException.StackTrace);
            }

            return strBuilder.ToString();
        }
    }
}