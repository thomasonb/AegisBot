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
        public override List<UInt64> Channels { get; set; } = new List<ulong>();
        internal override DiscordClient Client { get; set; }
        public override string HelpText { get; set; }

        public ManagerService()
        {

        }

        public override void LoadCommands()
        {
            if (CommandList != null)
            {
                return;
            }
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
                    },
                    HelpText = $"```{Environment.NewLine}.addservice - usage {Environment.NewLine}" +
                               $".addservice (servicename) {Environment.NewLine}" +
                               $"Type .listservices in order to get a list of the enabled services for your role(s).{Environment.NewLine}" +
                               $"```"
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
                },
                new CommandInfo("readyService")
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
                new CommandInfo("addCommandRole")
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
                            IsRequired = true
                        },
                        new ParameterInfo()
                        {
                            ParameterIndex = 2,
                            ParameterName = "RoleName",
                            IsRequired = true
                        }
                    },
                    HelpText = $"```{Environment.NewLine}.addCommandRole - usage {Environment.NewLine}" +
                               $".addcommandrole (servicename) (commandname) (rolename) {Environment.NewLine}" +
                               $"Type .listservices in order to get a list of the enabled services for your role(s).{Environment.NewLine}" +
                               $"```"
                }
            };
            SaveService();
        }

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
                return StartCommand(e, e.User);
            }
            return Task.FromResult<object>(null);
        }

        private async Task<Message> AddService(List<string> Parameters, Message Message)
        {
            if (Parameters.Any())
            {
                var service = ServiceFactory.GetService(Parameters[0]) as AegisService;
                if (service != null)
                {
                    service.Channels.Add(Message.Channel.Id);
                    service.SaveService();
                    return await Message.Channel.SendMessage($"{Parameters[0]} is now listening to this channel");
                }
                else
                {
                    return await Message.Channel.SendMessage($"{Parameters[0]} is not a valid service");
                }
            }
            else
            {
                return await GetServiceHelp(GetParametersFromMessage("!help addservice"), Message.User);
            }
        }

        private async Task<Message> ReadyService(List<string> Parameters, Message Message)
        {
            var service = ServiceFactory.GetService(Parameters[0]) as AegisService;
            if (service != null)
            {
                service.state = ServiceState.Ready;
                service.SaveService();
                return await Message.Channel.SendMessage($"{Parameters[0]} is now ready and will start accepting commands.");
            }
            else
            {
                return await GetServiceHelp(GetParametersFromMessage("!help addservice"), Message.User);
            }
        }

        private async Task<Message> AddCommandRole(List<string> Parameters, Message Message)
        {
            var service = ServiceFactory.GetService(Parameters[0]) as AegisService;

            string result = "";

            if (service != null)
            {
                var command = service.CommandList.FirstOrDefault(x => x.CommandName == Parameters[1]);
                if (command != null)
                {
                    Role role = Message.Server.Roles.FirstOrDefault(x => x.Name == Parameters[2]);
                    if (role != null)
                    {
                        command.Roles.Add(new RoleInfo() { RoleID = role.Id, RoleName = role.Name });
                        result = $"{role.Name} is now able to use {command.CommandName}";
                        service.SaveService();
                    }
                    else
                    {
                        result = $"{Parameters[2]} is not a valid role in this server";
                    }
                }
                else
                {
                    result = $"{Parameters[1]} is not a valid command in {service.GetType().Name} in this server";
                }
            }
            else
            {
                result = $"{Parameters[0]} is not a valid service in this server";
            }

            return await Message.Channel.SendMessage(result);
        }

        private async Task<Message> GetServiceHelp(List<string> Parameters, User User)
        {
            AegisService x = (ServiceFactory.GetService(Parameters[0]) as AegisService);
            if (x != null)
            {
                return await User.SendMessage(x.HelpText);
            }
            return null;
        }

        private async Task<Message> GetCommandHelp(CommandInfo command, User User)
        {
            return await User.SendMessage(command.HelpText);
        }

        private async Task<Message> StartCommand(MessageEventArgs e, User user)
        {
            CommandInfo command = GetCommandFromMessage(e.Message.Text);
            List<string> paramList = GetParametersFromMessage(e.Message.Text);
            if (CanRunCommand(command, user))
            {
                if (FillParameterValues(GetParametersFromMessage(e.Message.Text), command))
                {
                    switch (command.CommandName.ToLower())
                    {
                        case "addservice":
                            return await AddService(paramList, e.Message);
                        case "servicehelp":
                            return await GetServiceHelp(paramList, user);
                        case "commandhelp":
                            return await GetCommandHelp(command, user);
                        case "readyservice":
                            return await ReadyService(paramList, e.Message);
                        case "addcommandrole":
                            return await AddCommandRole(paramList, e.Message);
                    }
                }
                else
                {
                    return await GetCommandHelp(command, user);
                }
            }
            else
            {
                //return await PermissionsIssue();
            }
            return null;
        }

        public override Task RunCommand(UserEventArgs e, string command)
        {
            throw new NotImplementedException();
        }
    }
}
