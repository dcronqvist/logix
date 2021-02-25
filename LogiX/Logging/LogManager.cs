using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LogiX.Logging
{
    static class LogManager
    {
        public static List<LogEntry> Entries { get; set; }

        static LogManager()
        {
            Entries = new List<LogEntry>();
        }

        public static void AddEntry(LogEntry entry)
        {
            Entries.Add(entry);
            Debug.WriteLine(entry.ToString());
        }

        public static void AddEntry(string text, LogEntryType type = LogEntryType.INFO)
        {
            AddEntry(text, DateTime.Now, type);
        }

        public static void AddEntry(string text, DateTime time, LogEntryType type = LogEntryType.INFO)
        {
            LogEntry le = new LogEntry(time, text, type);
            AddEntry(le);
        }
    }
}
