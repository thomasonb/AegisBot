using AegisBotV2.Implementations;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AegisBotV2.Services
{
    public static class ApplicationService
    {
        public static string saveDir = new DirectoryInfo(Assembly.GetEntryAssembly().Location).Parent?.Parent?.Parent?.Parent?.FullName + "\\Applications";

        public static async Task SendApplication(string message, IUser user)
        {
            Application app = new Application(user.Id, true)
            {
                ApplicationTitle = "__My Title__"
            };

            await app.SaveApplication(saveDir);
            app.CurrentState = Application.State.New;
            IDMChannel tempChannel = await user.CreateDMChannelAsync();
            app.ChannelID = tempChannel.Id;
            await app.SaveApplication(saveDir);
            await tempChannel.SendMessageAsync(app.GetApplication(false));

            await tempChannel.SendMessageAsync($"Hello {user.Username}, Welcome to Gaming. Please complete the application above in order for the mods to determine your role in the server.{Environment.NewLine}{Environment.NewLine}" +
                                 $"Type !begin whenever you are ready to begin the application process. At the end you will be able to review the application and make changes if necessary");
        }

        public static async Task BeginApplication(CommandContext ctx)
        {
            Application app =
                GetApplicationsForChannel(ctx.Channel.Id, new List<Application.State> { Application.State.New }).First(x => x.UserID == ctx.User.Id);
            app.CurrentState = Application.State.InProgress;
            await app.SaveApplication(saveDir);
            await AskNextQuestion(ctx, new List<Application.State>() { Application.State.InProgress });
        }

        public static async Task ReviewApplication(CommandContext ctx)
        {
            Application app = GetApplicationsForChannel(ctx.Channel.Id, new List<Application.State>() { Application.State.Any }).First(x => x.UserID == ctx.User.Id);
            app.CurrentState = Application.State.Review;
            await app.SaveApplication(saveDir);
            await ctx.Channel.SendMessageAsync(app.GetApplication(false));
            await ctx.Channel.SendMessageAsync(
                                $"Above is the application you are about to send. Please review your answers and change any you want by typing !change (question number) without the parentheses.{Environment.NewLine}{Environment.NewLine}" +
                                $"Once you are done making your modifications, please type !submit to submit your application.");

        }

        public static async Task AskNextQuestion(CommandContext ctx, List<Application.State> states)
        {
            Application app = GetApplicationsForChannel(ctx.Channel.Id, states).First(x => x.UserID == ctx.User.Id);
            await ctx.Channel.SendMessageAsync(app.QAs.First(x => x.QuestionID == app.CurrentQuestionID + 1).Question);
        }

        public static async Task SubmitApplication(CommandContext ctx)
        {
            Application app = GetApplicationsForChannel(ctx.Channel.Id, new List<Application.State>() { Application.State.Finished, Application.State.Review }).FirstOrDefault(x => x.UserID == ctx.User.Id);

            app.CurrentState = Application.State.Submitted;
            await app.SaveApplication(saveDir);
            await ctx.Channel.SendMessageAsync(
                "Your application has been submitted for approval. You will be notified when it's status has been updated.");

            IGuild tempGuild = ctx.Client.GetGuildsAsync().Result.FirstOrDefault(x => x.Name == "ybadragon");
            IReadOnlyCollection<IGuildChannel> tempChannels = await tempGuild?.GetChannelsAsync();
            ITextChannel tempChannel = tempChannels.FirstOrDefault(x => x.Name == "applications") as ITextChannel;
            if (tempChannel == null)
            {
                RequestOptions options = new RequestOptions();
                tempChannel = await tempGuild?.CreateTextChannelAsync("applications");
                //owner, admin, moderator, moderator in training
                OverwritePermissions perms = new OverwritePermissions(readMessageHistory: PermValue.Allow, sendMessages: PermValue.Allow, readMessages: PermValue.Allow);
                List<string> RoleNames = new List<string>() { "Owner", "Admin", "Moderator", "Moderator in training", "Aegis", "Developer", "Mods" };
                IReadOnlyCollection<IRole> roles = tempGuild?.Roles.Where(x => RoleNames.Contains(x.Name)).ToList();
                roles.ToList().ForEach(x =>
                {
                    tempChannel.AddPermissionOverwriteAsync(x, perms);
                });
            }
            await tempChannel.SendMessageAsync(app.GetApplication(true));
            await tempChannel.SendMessageAsync(
                $"To approve/deny/investigate further this application type !approve/!deny/!investigate (ApplicationID) without the parentheses." +
                "This will message the user explaining the current status of their application.");
        }

        public static List<Application> GetApplicationsForChannel(ulong channelId, List<Application.State> state)
        {
            List<string> ApplicationFiles = Directory.GetFiles(saveDir).ToList();
            List<Application> Applications = new List<Application>();
            ApplicationFiles.ForEach(x =>
            {
                using (StreamReader sr = new StreamReader(new FileStream(x, FileMode.Open)))
                {
                    Applications.Add(JsonConvert.DeserializeObject<Application>(sr.ReadToEnd()));
                }
            });

            return Applications.Where(x => state.Contains(x.CurrentState) || state.Contains(Application.State.Any)).ToList();
        }
    }
}
