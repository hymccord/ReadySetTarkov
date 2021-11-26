using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ReadySetTarkov.LogReader
{
    [DebuggerDisplay("{Line}")]
    public class LogLine
    {
        public LogLine(string ns, string line)
        {
            Namespace = ns;
            Line = line;
            var regex = new Regex(@"^(?<date>.*?)\|(?<gameversion>.*?)\|(?<level>.*?)\|(?<logger>.*?)\|(?<message>.*)");
            var match = regex.Match(line);
            if (match.Success)
            {
                var ts = match.Groups["date"].Value;
                if (DateTime.TryParse(ts, out DateTime time))
                {
                    Time = time;
                }

                LineContent = match.Groups["message"].Value;
            }
        }

        public string Namespace { get; set; }
        public DateTime Time { get; } = DateTime.Now;
        public string Line { get; set; }
        public string? LineContent { get; set; }
    }
}