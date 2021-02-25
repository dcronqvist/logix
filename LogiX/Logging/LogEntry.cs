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

        public override string ToString()
        {
            // [2021-02-25 23:27 INFO]: This is some random log text
            string dateTime = Time.ToString("G");
            return $"[{dateTime} {Type}]: {Text}";
        }
    }
}
