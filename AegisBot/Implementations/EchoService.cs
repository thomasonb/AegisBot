using AegisBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace AegisBot.Implementations
{
    class EchoService : AegisService, IAegisService
    {
        public override DiscordClient Client { get; set; }

        public override string CommandDelimiter { get; set; } = "!";

        public override List<string> CommandList { get; set; }

        public override void HandleEvents()
        {
            Client.MessageReceived += async (s, e) => 
            { 
                if (!e.Message.IsAuthor)
                {
                    await RunCommand(e);
                }
            };
        }

        public override Task RunCommand(MessageEventArgs e)
        {
            var y = Client.Servers.First(x => x.Name == "myServer").TextChannels.First(x => x.Name == "closedchannel");
            return y.SendMessage(e.Message.Text);
        }

        public override Task RunCommand(UserEventArgs e, string command)
        {
            throw new NotImplementedException();
        }
    }
}
