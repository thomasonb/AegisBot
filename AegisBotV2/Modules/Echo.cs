using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AegisBotV2.Modules
{
    public class Echo : ModuleBase
    {
        [Command("Say", RunMode = RunMode.Async), Summary("Echos a message.")]
        public async Task Say([Remainder, Summary("The text to echo")] string text)
        {
            await ReplyAsync(text);
        }
    }
}
