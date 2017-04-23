using AegisBotV2.Modules;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AegisBotV2
{
    public class Program
    {
        DiscordSocketClient client;
        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();
        public DependencyMap map = new DependencyMap();
        public CommandService commands = new CommandService();
        private string saveDir = new DirectoryInfo(Assembly.GetEntryAssembly().Location).Parent?.Parent?.Parent?.Parent?.Parent?.FullName;


        public async Task MainAsync()
        {
            map = new DependencyMap();
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
            });
            client.Log += Log;

            string token = GetAuthToken();

            await InstallCommands();

            client.UserJoined += UserJoined;
            //client.Ready += LoggedIn;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task LoggedIn()
        {
            await UserJoined(client.Guilds.First().Users.First(x => x.Username == "ybadragon"));
        }

        public async Task InstallCommands()
        {
            client.MessageReceived += MessageReceived;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            IDMChannel tempChannel = await user.CreateDMChannelAsync();
            IUserMessage msg = await tempChannel.SendMessageAsync("Initializing Message Connection");
            ICommandContext ctx = new CommandContext(client, msg);
            CommandInfo cmd = commands.Commands.FirstOrDefault(x => x.Name == "Apply");
            var result = await cmd.ExecuteAsync(ctx, new List<object>(), new List<object>(), map);
            if (!result.IsSuccess)
            {
                await ctx.Channel.SendMessageAsync(result.ErrorReason);
            }
            await msg.DeleteAsync();
        }

        private async Task MessageReceived(SocketMessage msg)
        {
            //Don't process the command if it was a System Message
            var message = msg as SocketUserMessage;
            if (message == null || message.Author.IsBot)
            {
                return;
            }
            //make a number that tracks where the prefix ends and the command begins
            int argPos = 0;
            if (message.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                //this removes the Mention tag, but replaces it with @UserName instead...
                //string clean = message.Resolve();
                string command = message.ToString().Substring(argPos).Trim();
                //List<string> Mentions = message.MentionedUsers.Select(x => x.Mention.Replace("!", "").ToString()).ToList();
                //string CleanMessage = "";
                //Mentions.ForEach(x =>
                //{
                //    CleanMessage = message.Content.Replace(x, "");
                //});
                //CleanMessage = CleanMessage.Trim();
                CommandContext ctx = new CommandContext(client, message);
                var result = await commands.ExecuteAsync(ctx, argPos, map);
                if (!result.IsSuccess)
                {
                    await ctx.Channel.SendMessageAsync(result.ErrorReason);
                }
            }
            else
            {
                return;
            }
        }

        private Task Log(LogMessage message)
        {
            var cc = Console.ForegroundColor;
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message}");
            Console.ForegroundColor = cc;
            return Task.CompletedTask;
        }

        public string GetAuthToken()
        {
            string AuthFile = Directory.GetFiles(saveDir).FirstOrDefault(x => x.Contains("Auth.txt"));

            string token;
            using (StreamReader sr = new StreamReader(new FileStream(saveDir + "\\Auth.txt", FileMode.Open)))
            {
                token = sr.ReadToEnd();
            }
            return token;
        }

        //private async Task InitCommands()
        //{
        //    map.Add()
        //}
    }
}
