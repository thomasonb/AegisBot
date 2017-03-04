using AegisBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace AegisBot.Implementations
{
    class EchoService : AegisService
    {
        public override DiscordClient Client { get; set; }
        public override string CommandDelimiter { get; set; } = "!";
        public override List<UInt64> Channels { get; set; }
        public override List<CommandInfo> CommandList { get; set; }
        public override string HelpText { get; set; }

        public override void HandleEvents()
        {
            Client.MessageReceived += async (s, e) =>
            {
                if (Channels.Contains(e.Channel.Id))
                {
                    if (!e.Message.IsAuthor)
                    {
                        await RunCommand(e);
                    }
                }

            };
        }

        public override Task<Message> RunCommand(MessageEventArgs e)
        {
            var y = Client.Servers.First(x => x.Name == "myServer").TextChannels.First(x => x.Name == "closedchannel");
            return y.SendMessage(e.Message.Text);
        }

        public override Task<Message> RunCommand(UserEventArgs e, string command)
        {
            throw new NotImplementedException();
        }
    }
}
