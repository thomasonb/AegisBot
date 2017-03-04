using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AegisBot.Implementations
{
    public class CommandInfo
    {
        public string CommandName { get; set; }
        public List<ChannelInfo> Channels { get; set; }
        public List<ParameterInfo> Parameters { get; set; } = new List<ParameterInfo>();
        public List<RoleInfo> Roles { get; set; }
        public string HelpText { get; set; }

        public CommandInfo(string commandName)
        {
            CommandName = commandName;
        }
    }
}
