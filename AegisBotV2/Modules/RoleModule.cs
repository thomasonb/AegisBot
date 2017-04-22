using AegisBotV2.Services;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AegisBotV2.Modules
{
    public class RoleModule : InteractiveModuleBase
    {
        [Command("SetRank", RunMode = RunMode.Async), Summary("Set the ranked role of the user")]
        public async Task SetRank([Remainder] string rankName)
        {
            await RoleService.SetRank(Context, rankName);
        }

        [Command("SetRegion", RunMode = RunMode.Async), Summary("Set the region role of the user")]
        public async Task SetRegion([Remainder] string regionName)
        {
            await RoleService.SetRegion(Context, regionName);
        }

        [Command("SetPlatform", RunMode = RunMode.Async), Summary("Set the platform role of the user")]
        public async Task SetPlatform([Remainder] string platformName)
        {
            await RoleService.SetPlatform(Context, platformName);
        }
    }
}
