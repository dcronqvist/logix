using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Logging
{
    enum LogEntryType
    {
        WARNING,
        INFO,
        ERROR
    }

    struct LogEntry
    {
        public DateTime Time { get; set; }
        public string Text { get; set; }
        public LogEntryType Type { get; set; }

        public LogEntry(DateTime time, string text, LogEntryType type)
        {
            this.Time = time;
            this.Text = text;
            this.Type = type;
        }
    }
}
