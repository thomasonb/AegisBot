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
        public abstract List<CommandInfo> CommandList { get; set; }
        public abstract string CommandDelimiter { get; set; }
        public abstract DiscordClient Client { get; set; }
        public abstract List<UInt64> Channels { get; set; }
        public abstract string HelpText { get; set; }

        public bool ContainsCommand(string message)
        {
            string command = message.Substring(0, message.Contains(" ") ? message.IndexOf(" ") : message.Length);
            if (CommandList.Any())
            {
                return CommandList.Any(x => CommandDelimiter + x == command.ToLower());
            }
            return false;
        }

        internal CommandInfo GetCommandFromMessage(string message)
        {
            return
                CommandList.First(
                    x =>
                        CommandDelimiter + x.CommandName ==
                        message.ToLower().Substring(0, message.Contains(" ") ? message.IndexOf(" ") : message.Length));
        }

        internal List<string> GetParametersFromMessage(string message)
        {
            Regex regex = new Regex(@"[^\s""']+|""([^""]*)""|'([^']*)'");
            return regex.Matches(message).Cast<Match>().Select(x => x.Value).ToList().Skip(1).ToList();
        }

        internal bool FillParameterValues(List<string> paramInfo, CommandInfo command)
        {
            paramInfo.ForEach(x =>
            {
                command.Parameters[paramInfo.IndexOf(x)].ParameterValue = x;
            });

            return command.Parameters.Any(x => x.IsRequired && string.IsNullOrWhiteSpace(x.ParameterValue));
        }

        public abstract Task<Message> RunCommand(MessageEventArgs e);

        public abstract Task<Message> RunCommand(UserEventArgs e, string command);

        public abstract void HandleEvents();
    }
}
