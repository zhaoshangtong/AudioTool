using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AudioToolNew.Common
{
    /// <summary>
    /// 日志类
    /// </summary>
    public class LogHelper
    {
        private static readonly log4net.ILog loginfo = log4net.LogManager.GetLogger("loginfo");
        private static readonly log4net.ILog logerror = log4net.LogManager.GetLogger("logerror");
        private static readonly log4net.ILog logmonitor = log4net.LogManager.GetLogger("logmonitor");


        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="errorMsg">错误信息</param>
        /// <param name="ex">异常</param>
        public static void Error(string errorMsg, Exception ex = null)
        {
            if (ex != null)
            {
                logerror.Error(errorMsg, ex);
            }
            else
            {
                logerror.Error(errorMsg);
            }

        }
        /// <summary>
        /// 普通日志
        /// </summary>
        /// <param name="msg">信息</param>
        public static void Info(string msg)
        {
            loginfo.Info(msg);
        }


        /// <summary>
        /// 监控日志
        /// </summary>
        /// <param name="msg">信息</param>
        public static void Monitor(string msg)
        {
            logmonitor.Info(msg);
        }

    }
}