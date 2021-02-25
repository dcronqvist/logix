using LogiX.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LogiX.Logging
{
    static class LogManager
    {
        public static List<LogEntry> Entries { get; set; }

        static LogManager()
        {
            Entries = new List<LogEntry>();
        }

        public static void DumpToFile()
        {
            // We do not need to check if the directory exists, it always will upon start up
            Entries.Sort((a, b) =>
            {
                if (a.Time > b.Time)
                    return 1;
                else
                    return -1;
            });

            using(StreamWriter sw = new StreamWriter(Utility.LOG_FILE, false))
            {
                for (int i = 0; i < Entries.Count; i++)
                {
                    LogEntry entry = Entries[i];

                    sw.WriteLine(entry.ToString());
                }
            }
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
