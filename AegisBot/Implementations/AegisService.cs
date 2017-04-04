using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using AegisBot.Interfaces;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace AegisBot.Implementations
{
    public abstract class AegisService : IAegisService
    {
        public enum ServiceState
        {
            NotReady, Ready
        }
        public abstract List<CommandInfo> CommandList { get; set; }
        public abstract string CommandDelimiter { get; set; }
        internal abstract DiscordClient Client { get; set; }
        public abstract List<UInt64> Channels { get; set; }
        public abstract string HelpText { get; set; }
        private string saveDir = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent?.Parent?.Parent?.FullName + "\\Services";
        private ServiceState? _state { get; set; }
        public ServiceState? state { get { return _state == null ? ServiceState.NotReady : _state; } set { _state = value; } }

        public async void SaveService()
        {
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            using (StreamWriter sw = new StreamWriter(saveDir + $"\\{GetType().Name}.Service.json", false))
            {
                await sw.WriteAsync(JsonConvert.SerializeObject(this, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.Indented }));
            }
        }

        public bool CanRunCommand(CommandInfo command, User user)
        {
            return (command.Roles.Count == 0 || user.Roles.Any(x => command.Roles.Any(y => y.RoleID == x.Id))) && !user.IsBot && (state == ServiceState.Ready || command.CommandName.ToLower() == "readyservice");
        }

        public bool ContainsCommand(string message)
        {
            string command = message.Substring(0, message.Contains(" ") ? message.IndexOf(" ") : message.Length);
            if (CommandList.Any())
            {
                return CommandList.Any(x => CommandDelimiter + x.CommandName.ToLower() == command.ToLower());
            }
            return false;
        }

        internal CommandInfo GetCommandFromMessage(string message)
        {
            return
                CommandList.First(
                    x =>
                        CommandDelimiter + x.CommandName.ToLower() ==
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

            //make sure all parameters that are required are filled out
            return !command.Parameters.Where(x => x.IsRequired).Any(x => string.IsNullOrWhiteSpace(x.ParameterValue));

            //return command.Parameters.Any(x => x.IsRequired && !string.IsNullOrWhiteSpace(x.ParameterValue))
        }

        public abstract void LoadCommands();

        public abstract Task RunCommand(MessageEventArgs e);

        public abstract Task RunCommand(UserEventArgs e, string command);

        public abstract void HandleEvents();
    }
}
