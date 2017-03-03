using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace AegisBot.Implementations
{
    class ManagerService : AegisService
    {
        public override List<string> CommandList { get; set; } = new List<string>() { "addService" };
        public override string CommandDelimiter { get; set; } = ".";
        public override List<UInt64> Channels { get; set; }
        public override DiscordClient Client { get; set; }

        public override void HandleEvents()
        {
            Client.MessageReceived += async (s, e) =>
            {
                await RunCommand(e);
            };
        }

        public override Task RunCommand(MessageEventArgs e)
        {
            if (ContainsCommand(e.Message.Text))
            {
                string Command = GetCommandFromMessage(e.Message.Text);
                List<string> Parameters = GetParametersFromMessage(e.Message.Text);
                if (Parameters.Any())
                {
                    var service = ServiceFactory.GetService(Parameters[0]) as AegisService;
                    if (service != null)
                    {
                        service.Channels.Add(e.Channel.Id);
                    }
                }
            }
            return null;
        }

        public override Task RunCommand(UserEventArgs e, string command)
        {
            throw new NotImplementedException();
        }
    }
}
