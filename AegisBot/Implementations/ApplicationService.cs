using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using AegisBot.Interfaces;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace AegisBot.Implementations
{
    public class ApplicationService : AegisService
    {
        public override List<string> CommandList { get; set; }
        public override string CommandDelimiter { get; set; }
        public override List<UInt64> Channels { get; set; }
        public override DiscordClient Client { get; set; }
        private static string saveDir = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent?.Parent?.Parent?.FullName + "\\Applications";

        public ApplicationService()
        {
            CommandDelimiter = "!";
            CommandList = new List<string> { "apply", "review", "change", "submit", "next", "begin", "approve", "deny", "investigate" };
        }

        public async Task<Message> SendApplication(string message, User user)
        {
            Application app = new Application(user.Id)
            {
                ApplicationTitle = "__My Title__"
            };

            await app.SaveApplication(saveDir);
            app.CurrentState = Application.State.New;
            Channel tempChannel = await user.CreatePMChannel();
            app.ChannelID = tempChannel.Id;
            await app.SaveApplication(saveDir);
            await tempChannel.SendMessage(app.GetApplication(false));

            return await tempChannel.SendMessage($"Hello {user.Name}, Welcome to Gaming. Please complete the application above in order for the mods to determine your role in the server.{Environment.NewLine}{Environment.NewLine}" +
                                 $"Type !begin whenever you are ready to begin the application process. At the end you will be able to review the application and make changes if necessary");

        }

        public async Task<Message> BeginApplication(User user)
        {
            Application app =
                GetApplicationsForChannel(user.PrivateChannel.Id, Application.State.New).First(x => x.UserID == user.Id);
            app.CurrentState = Application.State.InProgress;
            await app.SaveApplication(saveDir);
            return await AskNextQuestion(user, Application.State.InProgress);
        }

        public async Task<Message> AskNextQuestion(User user, Application.State state)
        {
            Application app = GetApplicationsForChannel(user.PrivateChannel.Id, state).First(x => x.UserID == user.Id);
            return await user.SendMessage(app.QAs.First(x => x.QuestionID == app.CurrentQuestionID + 1).Question);
        }

        public async Task<Message> AskQuestion(User user, int questionID)
        {
            Application app = GetApplicationsForChannel(user.PrivateChannel.Id, Application.State.Any).First(x => x.UserID == user.Id);
            app.CurrentQuestionID = questionID - 1;
            app.CurrentState = Application.State.Change;
            await app.SaveApplication(saveDir);
            return await AskNextQuestion(user, Application.State.Change);
        }

        public async Task<Message> ReviewApplication(User user)
        {
            Application app = GetApplicationsForChannel(user.PrivateChannel.Id, Application.State.Any).First(x => x.UserID == user.Id);
            await user.SendMessage(app.GetApplication(false));
            return await user.SendMessage(
                                $"Above is the application you are about to send. Please review your answers and change any you want by typing !change (question number) without the parentheses.{Environment.NewLine}{Environment.NewLine}" +
                                $"Once you are done making your modifications, please type !submit to submit your application.");

        }

        public async Task<Message> ChangeQuestion(User user, string questionNumber)
        {
            int questionID;
            if (int.TryParse(questionNumber, out questionID))
            {
                return await AskQuestion(user, questionID);
            }
            return null;
        }

        public async Task<Message> SubmitApplication(User user, Channel channel)
        {
            Application app = GetApplicationsForChannel(user.PrivateChannel.Id, Application.State.Finished).FirstOrDefault(x => x.UserID == user.Id);
            if (app == null)
            {
                return null;
            }
            app.CurrentState = Application.State.Submitted;
            await app.SaveApplication(saveDir);
            await user.SendMessage(
                "Your application has been submitted for approval. You will be notified when it's status has been updated.");
            await channel.SendMessage(app.GetApplication(true));
            return
                await channel.SendMessage(
                    $"To approve/deny/investigate further this application type !approve/!deny/!investigate (ApplicationID) without the parentheses." +
                    "This will message the user explaining the current status of their application.");
        }

        public async Task<Message> DenyApplication(User user, string applicationID, Channel channel)
        {
            if (user.Roles.Any(x => x.Name == "Mods" && x.Client.Servers.Select(y => y.Name).Contains("myServer")))
            {
                Application app = GetApplicationByID(applicationID);
                if (app != null)
                {
                    app.CurrentState = Application.State.Denied;
                    await app.SaveApplication(saveDir);
                    User applicant = user.Client.Servers.First(x => x.Name == "myServer")
                        .Users.FirstOrDefault(x => x.Id == app.UserID);
                    if (applicant != null)
                    {
                        await channel.SendMessage($"{user.Name} has denied {applicant.Name}'s application. {applicant.Name} has been notified about the status change of their application.");

                        return
                            await applicant.PrivateChannel.SendMessage(
                                $"Unfortunately {applicant.Name} Your application has been denied by a Mod.");
                    }
                }
            }
            return null;
        }

        public async Task<Message> InvestigateApplication(User user, string applicationID, Channel channel)
        {
            if (user.Roles.Any(x => x.Name == "Mods" && x.Client.Servers.Select(y => y.Name).Contains("myServer")))
            {
                Application app = GetApplicationByID(applicationID);
                if (app != null)
                {
                    app.CurrentState = Application.State.Approved;
                    await app.SaveApplication(saveDir);
                    User applicant = user.Client.Servers.First(x => x.Name == "myServer")
                        .Users.FirstOrDefault(x => x.Id == app.UserID);
                    if (applicant != null)
                    {
                        await channel.SendMessage($"{user.Name} has marked {applicant.Name}'s application for further investigation. {applicant.Name} has been notified about the status change of their application.");

                        return
                            await applicant.PrivateChannel.SendMessage(
                                $"Your application has been marked for investigation by a Mod.");
                    }
                }
            }
            return null;
        }

        public async Task<Message> ApproveApplication(User user, string applicationID, Channel channel)
        {
            if (user.Roles.Any(x => x.Name == "Mods" && x.Client.Servers.Select(y => y.Name).Contains("myServer")))
            {
                Application app = GetApplicationByID(applicationID);
                if (app != null)
                {
                    app.CurrentState = Application.State.Approved;
                    await app.SaveApplication(saveDir);
                    User applicant = user.Client.Servers.First(x => x.Name == "myServer")
                        .Users.FirstOrDefault(x => x.Id == app.UserID);
                    if (applicant != null)
                    {
                        await applicant.AddRoles(applicant.Client.Servers.First(y => y.Name == "myServer").Roles.First(y => y.Name == "Approved"));
                        await channel.SendMessage($"{user.Name} has approved {applicant.Name}'s application. {applicant.Name} has been notified about the status change of their application.");
                        return
                            await applicant.PrivateChannel.SendMessage(
                                $"Congratulations {applicant.Name}! Your application has been approved by a Mod.");
                    }
                }
            }
            return null;
        }

        private Application GetApplicationByID(string applicationId)
        {
            List<string> ApplicationFiles = Directory.GetFiles(saveDir).ToList();
            List<Application> Applications = new List<Application>();
            ApplicationFiles.ForEach(x =>
            {
                using (StreamReader sr = new StreamReader(x))
                {
                    Applications.Add(JsonConvert.DeserializeObject<Application>(sr.ReadToEnd()));
                }
            });

            return Applications.FirstOrDefault(x => x.ApplicationID == applicationId);
        }

        public override Task RunCommand(MessageEventArgs e)
        {
            return StartCommand(e.Message.Text, e.User);
        }

        public override Task RunCommand(UserEventArgs e, string command)
        {
            if (ContainsCommand(command))
            {
                return StartCommand(command, e.User);
            }
            return null;
        }

        public override void HandleEvents()
        {
            Client.MessageReceived += async (s, e) =>
            {
                if (Channels.Contains(e.Channel.Id))
                {
                    //does this channel and user correspond to any applications?
                    Application app =
                        GetApplicationsForChannel(e.Channel.Id, Application.State.Any)
                            .FirstOrDefault(x => x.UserID == e.User.Id);
                    if (!e.Message.IsAuthor && e.Channel.IsPrivate && app != null)
                    {

                        //string command = e.Message.Text.Substring(0, e.Message.Text.Contains(" ") ? e.Message.Text.IndexOf(" ") : e.Message.Text.Length);
                        List<IAegisService> services = ServiceFactory.Services;

                        //is the user trying to run a command?
                        if (ContainsCommand(e.Message.Text))
                        {
                            await RunCommand(e);
                        }
                        //the user might be answering a question
                        else
                        {
                            //if the app is in progress the user is probably answering a question
                            if (app.CurrentState == Application.State.InProgress || app.CurrentState == Application.State.Change)
                            {
                                await app.AnswerQuestion(app.CurrentQuestionID + 1, e.Message.Text);
                                IAegisService applicationService = ServiceFactory.GetService<ApplicationService>();

                                //if the app is in a finished state let the user review the app and make any necessary changes
                                if (app.CurrentState == Application.State.Finished)
                                {
                                    await RunCommand(new UserEventArgs(e.User), "!review");
                                }
                                //otherwise ask the next question
                                else
                                {
                                    await RunCommand(new UserEventArgs(e.User), "!next");
                                }
                            }
                        }
                    }
                    //are there any services that have a command corresponding to this one
                    else if (!e.Message.IsAuthor)
                    {
                        if (ContainsCommand(e.Message.Text))
                        {
                            await RunCommand(e);
                        }
                    }
                }
            };

            Client.UserJoined += async (s, e) =>
            {
                IAegisService applicationService = ServiceFactory.GetService<ApplicationService>();
                await applicationService.RunCommand(e, "!apply");
            };
        }

        private async Task<Message> StartCommand(string message, User user)
        {
            Channel applicationChannel =
                Client.Servers.First(x => x.Name == "myServer").TextChannels.First(x => x.Name == "applications");
            string command = GetCommandFromMessage(message);
            List<string> paramList = GetParametersFromMessage(message);
            switch (command)
            {
                case "apply":
                    return await SendApplication(message, user);
                case "next":
                    return await AskNextQuestion(user, Application.State.InProgress);
                case "begin":
                    return await BeginApplication(user);
                case "review":
                    return await ReviewApplication(user);
                case "change":
                    return await ChangeQuestion(user, paramList[0]);
                case "submit":
                    return await SubmitApplication(user, applicationChannel);
                case "approve":
                    return await ApproveApplication(user, paramList[0], applicationChannel);
                case "deny":
                    return await DenyApplication(user, paramList[0], applicationChannel);
                case "investigate":
                    return await InvestigateApplication(user, paramList[0], applicationChannel);
            }
            return null;
        }

        public static List<Application> GetApplicationsForChannel(ulong channelId, Application.State state)
        {
            List<string> ApplicationFiles = Directory.GetFiles(saveDir).ToList();
            List<Application> Applications = new List<Application>();
            ApplicationFiles.ForEach(x =>
            {
                using (StreamReader sr = new StreamReader(x))
                {
                    Applications.Add(JsonConvert.DeserializeObject<Application>(sr.ReadToEnd()));
                }
            });

            return Applications.Where(x => x.CurrentState == state || state == Application.State.Any).ToList();
        }
    }
}
