using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Core.Common
{
    /// <summary>
    /// 日志帮助类
    /// </summary>
    public class LogHelper
    {
        /// <summary>
        /// log对象
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("Loggering");

        private LogHelper()
        { }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">消息</param>
        public static void Write(LogLevel level, string message)
        {
            Write(level, message, null, null);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="e">异常对象</param>
        public static void Write(LogLevel level, Exception e)
        {
            Write(level, null, null, e);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">消息</param>
        /// <param name="e">异常对象</param>
        public static void Write(LogLevel level, string message, Exception e)
        {
            Write(level, message, null, e);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">消息</param>
        /// <param name="additionalInfo">附加信息</param>
        public static void Write(LogLevel level, string message, IDictionary<string, string> additionalInfo)
        {
            Write(level, message, additionalInfo, null);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">消息</param>
        /// <param name="additionalInfo">附加信息</param>
        /// <param name="e">异常对象</param>
        public static void Write(LogLevel level, string message, IDictionary<string, string> additionalInfo, Exception e)
        {
            string formattedMessage;
            switch (level)
            {
                case LogLevel.Debug:
                    if (log.IsDebugEnabled)
                    {
                        formattedMessage = FormatOutputMessage(message, additionalInfo);
                        log.Debug(formattedMessage, e);
                    }
                    break;
                case LogLevel.Info:
                    if (log.IsInfoEnabled)
                    {
                        formattedMessage = FormatOutputMessage(message, additionalInfo);
                        log.Info(formattedMessage, e);
                    }
                    break;
                case LogLevel.Warning:
                    if (log.IsWarnEnabled)
                    {
                        formattedMessage = FormatOutputMessage(message, additionalInfo);
                        log.Warn(formattedMessage, e);
                    }
                    break;
                case LogLevel.Error:
                    if (log.IsErrorEnabled)
                    {
                        formattedMessage = FormatOutputMessage(message, additionalInfo);
                        log.Error(formattedMessage, e);
                    }
                    break;
                case LogLevel.Fatal:
                    if (log.IsFatalEnabled)
                    {
                        formattedMessage = FormatOutputMessage(message, additionalInfo);
                        log.Fatal(formattedMessage, e);
                    }
                    break;
                default:
                    throw new ArgumentException(string.Format("type '{0}' not add to Write method", level));
            }
        }

        /// <summary>
        /// 格式化输出消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="additionalInfo">附加信息</param>
        /// <returns>格式化后的消息体</returns>
        private static string FormatOutputMessage(string message, IDictionary<string, string> additionalInfo)
        {
            if (additionalInfo == null || additionalInfo.Count == 0)
                return message;
            System.Text.StringBuilder buffer = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(message))
                buffer.AppendLine(message);
            foreach (KeyValuePair<string, string> kvp in additionalInfo)
                buffer.AppendLine(kvp.Key + " = " + kvp.Value);
            return buffer.ToString();
        }
    }
}
