//   Copyright 2017-2019 Davinci Codes
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Also use the software for non-commercial purposes.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
using System;

namespace Pandora.Client.Universal
{
    public static class Log
    {
        public const int SE_ID_Critical_Error = 1500;
        public const int SE_ID_Stream_Error = 2000;
        public const int SE_ID_Information = 3000;

        public static void InitLog(FixedLengthLogFileControl aLog, string aFileName, ushort aLineLength, ushort aMaxSize, string aLogLevelFlag)
        {
            Log.LogLevelFlag = (LogLevelFlags)Enum.Parse(typeof(LogLevelFlags), aLogLevelFlag);
            aLog.FileName = aFileName;
            aLog.LineLength = aLineLength;
            aLog.MaxSize = aMaxSize;
            aLog.Active = true;
            Log.SystemLog = aLog;
        }

        static Log()
        {
            ThrowSystemLogMissingException = false;
            LogLevelFlag = LogLevelFlags.All;
        }

        /// <summary>
        /// any object that implements SystemLog will be used.
        /// </summary>
        public static ISystemLog SystemLog { get; set; }

        public static bool ThrowSystemLogMissingException { get; set; }
        public static LogLevelFlags LogLevelFlag { get; set; }

        public static void Write(LogLevel aLogLevel, string aText, params object[] aArgs)
        {
            if (LogLevelFlag.HasFlag(LogLevelFlags.All) ||
                (aLogLevel == LogLevel.Info && LogLevelFlag.HasFlag(LogLevelFlags.Information)) ||
                (aLogLevel == LogLevel.Warning && LogLevelFlag.HasFlag(LogLevelFlags.Warning)) ||
                (aLogLevel == LogLevel.Error && LogLevelFlag.HasFlag(LogLevelFlags.Error)) ||
                (aLogLevel == LogLevel.Critical) ||
                (aLogLevel == LogLevel.Debug && LogLevelFlag.HasFlag(LogLevelFlags.Debug))
            )
            {
                Write(string.Format("[{0}]:{1}", aLogLevel.ToString(), aText), aArgs);
            }
        }

        public static void Write(LogLevel aLogLevel, string aText)
        {
            Write(aLogLevel, "{0}", aText);
        }

        public static void Write(string aText)
        {
            Write("{0}", aText);
        }

        public static void Write(string aText, params object[] aArgs)
        {
            if (SystemLog != null)
            {
                SystemLog.Write(aText, aArgs);
            }
            else if (ThrowSystemLogMissingException)
            {
                throw new SystemLogMissingException();
            }
        }

        public static string EventSource = null;

        public static void WriteAppEvent(string aMsg, System.Diagnostics.EventLogEntryType aType, int aEventId)
        {
            string lLog = "Application";
            if (!string.IsNullOrEmpty(EventSource))
            {
                if (!System.Diagnostics.EventLog.SourceExists(EventSource))
                {
                    System.Diagnostics.EventLog.CreateEventSource(EventSource, lLog);
                }

                System.Diagnostics.EventLog.WriteEntry(EventSource, aMsg, aType, aEventId);
            }
        }
    }

    public enum LogLevel { Info, Warning, Error, Debug, Critical }

    [Flags]
    public enum LogLevelFlags { All = 15, Information = 1, Warning = 2, Error = 4, Debug = 8 }

    public interface ISystemLog
    {
        void Write(string aText, params object[] aArgs);
    }
}