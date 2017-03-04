using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using AegisBot.Interfaces;

namespace AegisBot.Implementations
{
    class ManagerService : AegisService
    {
        public override List<CommandInfo> CommandList { get; set; }
        public override string CommandDelimiter { get; set; } = ".";
        public override List<UInt64> Channels { get; set; }
        public override DiscordClient Client { get; set; }
        public override string HelpText { get; set; }

        public ManagerService()
        {
            CommandList = new List<CommandInfo>()
            {
                new CommandInfo("addservice")
                {
                    Parameters = new List<ParameterInfo>()
                    {
                        new ParameterInfo()
                        {
                            ParameterIndex = 0,
                            ParameterName = "ServiceName",
                            IsRequired = true
                        }
                    }
                },
                new CommandInfo("serviceHelp")
                {
                    Parameters = new List<ParameterInfo>()
                    {
                        new ParameterInfo()
                        {
                            ParameterIndex = 0,
                            ParameterName = "ServiceName",
                            IsRequired = true
                        }
                    }
                },
                new CommandInfo("addcommand")
                {
                    Parameters = new List<ParameterInfo>()
                    {
                        new ParameterInfo()
                        {
                            ParameterIndex = 0,
                            ParameterName = "ServiceName",
                            IsRequired = true
                        },
                        new ParameterInfo()
                        {
                            ParameterIndex = 1,
                            ParameterName = "CommandName",
                            IsRequired = false
                        }
                    }
                },
                new CommandInfo("commandHelp")
                {
                    Parameters = new List<ParameterInfo>()
                    {
                        new ParameterInfo()
                        {
                            ParameterIndex = 0,
                            ParameterName = "ServiceName",
                            IsRequired = true
                        },
                        new ParameterInfo()
                        {
                            ParameterIndex = 1,
                            ParameterName = "CommandName",
                            IsRequired = false
                        }
                    }
                }
            };
        }

        public override void HandleEvents()
        {
            Client.MessageReceived += async (s, e) =>
            {
                await RunCommand(e);
            };
        }

        public override Task<Message> RunCommand(MessageEventArgs e)
        {
            if (ContainsCommand(e.Message.Text))
            {
                return StartCommand(e, e.User);
            }
            return null;
        }

        private async Task<Message> AddService(List<string> Parameters, Message Message)
        {
            if (Message.User.Roles.Any(x => x.Name == "Mod"))
            {
                if (Parameters.Any())
                {
                    var service = ServiceFactory.GetService(Parameters[0]) as AegisService;
                    if (service != null)
                    {
                        service.Channels.Add(Message.Channel.Id);
                        return await Message.Channel.SendMessage($"{Parameters[0]} is now listening to this channel");
                    }
                    else
                    {
                        return await Message.Channel.SendMessage($"{Parameters[0]} is not a valid service");
                    }
                }
                else
                {
                    return await GetHelp(GetParametersFromMessage("!help addservice"), Message.User);
                }
            }
            else
            {
                return await Message.User.SendMessage($"You do not have permission to run that command");
            }
        }

        private async Task<Message> GetHelp(List<string> Parameters, User User)
        {
            AegisService x = (ServiceFactory.GetService(Parameters[0]) as AegisService);
            if (x != null)
            {
                return await User.SendMessage(x.HelpText);
            }
            return null;
        }

        private async Task<Message> StartCommand(MessageEventArgs e, User user)
        {
            CommandInfo command = GetCommandFromMessage(e.Message.Text);
            List<string> paramList = GetParametersFromMessage(e.Message.Text);
            switch (command.CommandName.ToLower())
            {
                case "addservice":
                    return await AddService(paramList, e.Message);
                case "servicehelp":
                    return await GetHelp(paramList, user);
            }
            return null;
        }

        public override Task<Message> RunCommand(UserEventArgs e, string command)
        {
            throw new NotImplementedException();
        }
    }
}
