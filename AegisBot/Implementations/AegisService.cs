using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using AegisBot.Interfaces;
using System.Text.RegularExpressions;

namespace AegisBot.Implementations
{
    public abstract class AegisService : IAegisService
    {
        public abstract List<string> CommandList { get; set; }
        public abstract string CommandDelimiter { get; set; }
        public abstract DiscordClient Client { get; set; }
        public abstract List<UInt64> Channels { get; set; }

        public bool ContainsCommand(string message)
        {
            string command = message.Substring(0, message.Contains(" ") ? message.IndexOf(" ") : message.Length);
            if (CommandList.Any())
            {
                return CommandList.Any(x => CommandDelimiter + x == command.ToLower());
            }
            return false;
        }

        internal string GetCommandFromMessage(string message)
        {
            return
                CommandList.First(
                    x =>
                        CommandDelimiter + x ==
                        message.ToLower().Substring(0, message.Contains(" ") ? message.IndexOf(" ") : message.Length));
        }

        internal List<string> GetParametersFromMessage(string message)
        {
            Regex regex = new Regex(@"[^\s""']+|""([^""]*)""|'([^']*)'");
            return regex.Matches(message).Cast<Match>().Select(x => x.Value).ToList().Skip(1).ToList();
        }

        public abstract Task RunCommand(MessageEventArgs e);

        public abstract Task RunCommand(UserEventArgs e, string command);

        public abstract void HandleEvents();
    }
}
