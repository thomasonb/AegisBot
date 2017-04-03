using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace AegisBot.Implementations
{
    public class TournamentService : AegisService
    {
        public override List<CommandInfo> CommandList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string CommandDelimiter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override List<ulong> Channels { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string HelpText { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        internal override DiscordClient Client { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void LoadCommands()
        {
            return;
        }

        public override void HandleEvents()
        {
            throw new NotImplementedException();
        }

        public override Task RunCommand(MessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override Task RunCommand(UserEventArgs e, string command)
        {
            throw new NotImplementedException();
        }
    }
}
