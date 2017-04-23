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

        public static async Task SendApplication(IUser user)
        {
            Application app = new Application(user.Id, true)
            {
                ApplicationTitle = "Libertas RL Application"
            };

            await app.SaveApplication(saveDir);
            app.CurrentState = Application.State.New;
            IDMChannel tempChannel = await user.CreateDMChannelAsync();
            app.ChannelID = tempChannel.Id;
            await app.SaveApplication(saveDir);
            await tempChannel.SendMessageAsync(app.GetApplication(false));

            await tempChannel.SendMessageAsync($"Hello {user.Username}, Welcome to Libertas RL! Please complete the application above to make it easier for you to find teammates!{Environment.NewLine}{Environment.NewLine}" +
                                 $"Type '@Aegis Begin' without the quotes whenever you are ready to begin the application process. At the end you will be able to review the application and make changes if necessary");
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
                                $"Above is the application you are about to send. Please review your answers and change any you want by typing '@Aegis Change (question number)' without the quotes or the parentheses.{Environment.NewLine}{Environment.NewLine}" +
                                $"Once you are done making your modifications, please type '@Aegis Submit' without the quotes to submit your application.");

        }

        public static async Task AskNextQuestion(CommandContext ctx, List<Application.State> states)
        {
            Application app = GetApplicationsForChannel(ctx.Channel.Id, states).First(x => x.UserID == ctx.User.Id);
            QA question = app.QAs[app.CurrentQuestionID];
            await ctx.Channel.SendMessageAsync(question.Question);
            if (question.ValidAnswers.Any())
            {
                string appendedAnswers = string.Join(", ", question.ValidAnswers);
                await ctx.Channel.SendMessageAsync($"{Environment.NewLine}Please choose from the following choices: {Environment.NewLine}{appendedAnswers}");
            }
        }

        public static async Task SubmitApplication(CommandContext ctx)
        {
            Application app = GetApplicationsForChannel(ctx.Channel.Id, new List<Application.State>() { Application.State.Finished, Application.State.Review }).FirstOrDefault(x => x.UserID == ctx.User.Id);

            app.CurrentState = Application.State.Submitted;
            await app.SaveApplication(saveDir);
            await ctx.Channel.SendMessageAsync(
                "Your application has been submitted for approval. You will be notified when it's status has been updated.");

            IGuild tempGuild = ctx.Guild;
            IReadOnlyCollection<IGuildChannel> tempChannels = await tempGuild?.GetChannelsAsync();
            ITextChannel tempChannel = tempChannels.FirstOrDefault(x => x.Name == "applications") as ITextChannel;
            if (tempChannel == null)
            {
                RequestOptions options = new RequestOptions();
                tempChannel = await tempGuild?.CreateTextChannelAsync("applications");
                //owner, admin, moderator, moderator in training
                OverwritePermissions perms = new OverwritePermissions(readMessageHistory: PermValue.Allow, sendMessages: PermValue.Allow, readMessages: PermValue.Allow);
                List<string> RoleNames = new List<string>() { "owner", "admin", "moderator", "moderator in training", "aegis", "developer", "mods" };
                IReadOnlyCollection<IRole> roles = tempGuild?.Roles.Where(x => RoleNames.Contains(x.Name.ToLower())).ToList();
                roles.ToList().ForEach(x =>
                {
                    tempChannel.AddPermissionOverwriteAsync(x, perms);
                });
            }
            await tempChannel.SendMessageAsync(app.GetApplication(true));
            await tempChannel.SendMessageAsync(
                $"To approve/deny/investigate further this application type '@Aegis Approve/Deny/Investigate (ApplicationID)' without the quotes or the parentheses. (note the ApplicationID can be found at the top of the Application)" +
                "This will message the user explaining the current status of their application.");
        }

        public static async Task DenyApplication(CommandContext ctx, string applicationID)
        {
            Application app = GetApplicationByID(applicationID);
            if (app != null)
            {
                app.CurrentState = Application.State.Denied;
                await app.SaveApplication(saveDir);
                IGuildUser applicant = await ctx.Guild.GetUserAsync(app.UserID);
                IDMChannel tempChannel = await applicant.CreateDMChannelAsync();
                await ctx.Channel.SendMessageAsync($"{ctx.User.Username} has denied {applicant.Username}'s application. {applicant.Username} has been notified about the status change of their application.");
                await tempChannel.SendMessageAsync($"Unfortunately {applicant.Username} Your application has been denied by a Mod.");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"{applicationID} is not a valid application.");
                //delete the application message if it no longer exists
            }
        }

        public static async Task InvestigateApplication(CommandContext ctx, string applicationID)
        {
            Application app = GetApplicationByID(applicationID);
            if (app != null)
            {
                app.CurrentState = Application.State.Denied;
                await app.SaveApplication(saveDir);
                IGuildUser applicant = await ctx.Guild.GetUserAsync(app.UserID);
                IDMChannel tempChannel = await applicant.CreateDMChannelAsync();
                await ctx.Channel.SendMessageAsync($"{ctx.User.Username} has marked {applicant.Username}'s application for further investigation. {applicant.Username} has been notified about the status change of their application.");

                await tempChannel.SendMessageAsync($"Your application has been marked for investigation by a Mod.");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"{applicationID} is not a valid application.");
                //delete the application message if it no longer exists
            }
        }

        public static async Task ApproveApplication(CommandContext ctx, string applicationID)
        {
            Application app = GetApplicationByID(applicationID);
            if (app != null)
            {
                app.CurrentState = Application.State.Approved;
                await app.SaveApplication(saveDir);
                IGuildUser applicant = await ctx.Guild.GetUserAsync(app.UserID);
                //User applicant = ctx.Client.GetGuildsAsync().Result.First(x => x.Name == "ybadragon")
                //    .Users.FirstOrDefault(x => x.Id == app.UserID);
                if (applicant != null)
                {
                    //add applicant to all roles associated with their answers
                    await applicant.AddRolesAsync(ctx.Guild.Roles.Where(x => app.QAs.Where(y => y.SetRoleToAnswer == true).Select(y => y.Answer.ToLower()).Contains(x.Name.ToLower())));
                    //await applicant.AddRolesAsync(ctx.Guild.Roles.Intersect(app.QAs.Where(y => y.SetRoleToAnswer).Select(y => y.Answer).ToList()));
                    await ctx.Channel.SendMessageAsync($"{ctx.User.Username} has approved {applicant.Username}'s application. {applicant.Username} has been notified about the status change of their application.");
                    IDMChannel tempChannel = await applicant.CreateDMChannelAsync();
                    await tempChannel.SendMessageAsync($"Congratulations {applicant.Username}! Your application has been approved by a Mod.");
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"{applicationID} is not a valid application.");
                //delete the application message if it no longer exists
            }
        }

        private static List<Application> GetApplications()
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
            return Applications;
        }

        public static List<Application> GetApplicationsForChannel(ulong channelId, List<Application.State> state)
        {
            List<Application> Applications = GetApplications();
            return Applications.Where(x => state.Contains(x.CurrentState) || state.Contains(Application.State.Any)).ToList();
        }

        public static Application GetApplicationByUser(ulong userId)
        {
            List<Application> Applications = GetApplications();
            return Applications.FirstOrDefault(x => x.UserID == userId);
        }

        public static Application GetApplicationByID(string applicationId)
        {
            List<Application> Applications = GetApplications();
            return Applications.FirstOrDefault(x => x.ApplicationID == applicationId);
        }
    }
}
