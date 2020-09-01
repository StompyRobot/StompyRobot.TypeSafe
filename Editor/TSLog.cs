//#define DEBUG_TRACE
#define ENABLE_LOGGING

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace TypeSafe.Editor
{
    internal enum LogCategory
    {
        // Debug trace
        Trace,

        // Info printed to the Unity console (for user to see)
        Info,

        // Trace information from the resource scanner
        Scanner,

        // Compile result logs
        Compile
    }

    internal static class TSLog
    {
        private static readonly Dictionary<LogCategory, List<LogEntry>> _logBuffers =
            new Dictionary<LogCategory, List<LogEntry>>();

        private static bool _hasAttemptedOpen;
        private static FileStream _fileStream;
        private static TextWriter _logWriter;

        public static void BeginBuffer(LogCategory category)
        {
            if (_logBuffers.ContainsKey(category))
            {
                return;
            }

            _logBuffers.Add(category, new List<LogEntry>());
        }

        public static void EndBuffer(LogCategory category, bool print = true)
        {
            List<LogEntry> buffer;

            if (!_logBuffers.TryGetValue(category, out buffer))
            {
                return;
            }

            _logBuffers.Remove(category);

            if (print)
            {
                for (var i = 0; i < buffer.Count; i++)
                {
                    Print(buffer[i].Type, buffer[i].Message, buffer[i].Target);
                }
            }

            buffer.Clear();
        }

        public static void Log(LogCategory category, string msg, Object target = null)
        {
            Log(category, LogType.Log, msg, target);
        }

        public static void LogWarning(LogCategory category, string msg, Object target = null)
        {
            Log(category, LogType.Warning, msg, target);
        }

        public static void LogError(LogCategory category, string msg, Object target = null)
        {
            Log(category, LogType.Error, msg, target);
        }

        private static void Log(LogCategory category, LogType type, string message, Object target = null)
        {
            LogToFile(CreateLogEntry(category, type.ToString(), message));

            List<LogEntry> buffer;

            if (_logBuffers.TryGetValue(category, out buffer))
            {
                buffer.Add(new LogEntry {Message = message, Type = type, Target = target});
            }

            if (!IsEnabled(category))
            {
                return;
            }

            Print(type, message, target);
        }

        private static void Print(LogType type, string message, Object target = null)
        {
            switch (type)
            {
                case LogType.Assert:
                case LogType.Error:
                case LogType.Exception:

                    Debug.LogError(Strings.LogPrefix + message, target);
                    break;

                case LogType.Warning:

                    Debug.LogWarning(Strings.LogPrefix + message, target);
                    break;

                default:

                    Debug.Log(Strings.LogPrefix + message, target);
                    break;
            }
        }

        [Conditional("ENABLE_LOGGING")]
        public static void CloseLog()
        {
            _logWriter.Dispose();
            _fileStream.Dispose();
            _hasAttemptedOpen = false;
        }

        private static bool IsEnabled(LogCategory c)
        {
#if !DEBUG_TRACE
            if (c != LogCategory.Info)
            {
                return false;
            }
#endif

            return true;
        }
        
        private static string CreateLogEntry(LogCategory category, string severity, string msg)
        {
	        return string.Format("{0,-8}\t{1,-8}\t{2}", severity, category, msg);
        }

        [Conditional("ENABLE_LOGGING")]
        private static void LogToFile(string msg)
        {
            if (!_hasAttemptedOpen)
            {
                var logFilePath = PathUtility.GetLogFilePath();

                try
                {
                    _fileStream = File.Open(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                    _logWriter = new StreamWriter(_fileStream, Encoding.UTF8);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error opening TypeSafe log file at " + logFilePath);
                    Debug.LogException(e);
                }

                _hasAttemptedOpen = true;
            }

            if (_logWriter == null)
            {
                return;
            }

            _logWriter.Write(DateTime.Now.ToString("s"));
            _logWriter.Write("\t");
            _logWriter.WriteLine(msg);

            _logWriter.Flush();
        }

        public struct LogEntry
        {
            public string Message;
            public Object Target;
            public LogType Type;
        }
    }
}
