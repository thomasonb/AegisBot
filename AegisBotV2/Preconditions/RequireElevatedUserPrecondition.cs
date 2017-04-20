using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AegisBotV2.Preconditions
{
    class RequireElevatedUserPrecondition : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            List<string> RoleNames = new List<string>() { "owner", "admin", "moderator", "moderator in training", "aegis", "developer", "mods" };
            IEnumerable<ulong> roleids = context.Guild.Roles.Where(x => RoleNames.Contains(x.Name.ToLower())).Select(x => x.Id);
            if ((context.User as IGuildUser).RoleIds.Intersect(roleids).Any())
            {
                return Task.FromResult((PreconditionResult.FromSuccess()));
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError(""));
            }
        }
    }
}
