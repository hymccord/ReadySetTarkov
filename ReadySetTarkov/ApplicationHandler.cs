using System;
using System.Text.RegularExpressions;

using ReadySetTarkov.LogReader;
using ReadySetTarkov.Utility;

namespace ReadySetTarkov
{
    internal class ApplicationHandler
    {
        private static readonly Regex GameRegex = new Regex(@"^Game(?<action>\w+)");
        public ApplicationHandler()
        {
        }

        internal void Handle(LogLine line, Game game)
        {
            if (string.IsNullOrEmpty(line.LineContent))
                return;

            if (GameRegex.IsMatch(line.LineContent))
            {
                var match = GameRegex.Match(line.LineContent);
                switch (match.Groups["action"].Value)
                {
                    case "Starting":
                        User32.FlashTarkov();
                        break;
                    case "Start":
                        //User32.BringTarkovToForeground();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}