using AegisBotV2.Implementations;
using AegisBotV2.Preconditions;
using AegisBotV2.Services;
using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AegisBotV2.Modules
{
    public class ApplicationModule : InteractiveModuleBase
    { 
        //new CommandInfo("approve") { Parameters = new List<ParameterInfo>() { new ParameterInfo() { ParameterIndex = 0, ParameterName = "ApplicationID", IsRequired = true } } },
        //new CommandInfo("deny") { Parameters = new List<ParameterInfo>() { new ParameterInfo() { ParameterIndex = 0, ParameterName = "ApplicationID", IsRequired = true } } },
        //new CommandInfo("investigate") { Parameters = new List<ParameterInfo>() { new ParameterInfo() { ParameterIndex = 0, ParameterName = "ApplicationID", IsRequired = true } } },

        [Command("Apply", RunMode = RunMode.Async), Summary("Sends an application to the user")]
        public async Task Apply()
        {
            await ApplicationService.SendApplication(Context.Message.Content, Context.User);
        }

        [Command("Begin", RunMode = RunMode.Async), Summary("Begins the users application"), RequireDMChannel]
        public async Task Begin()
        {
                //await ApplicationService.BeginApplication(Context);
                await Next();
        }

        [Command("Next", RunMode = RunMode.Async), Summary("Sends the next question to the user"), RequireDMChannel]
        public async Task Next()
        {
                Application app = ApplicationService.GetApplicationsForChannel(Context.Channel.Id, new List<Application.State> { Application.State.InProgress, Application.State.Change, Application.State.Finished, Application.State.New }).FirstOrDefault(x => x.UserID == Context.User.Id);
                if (app.CurrentState == Application.State.InProgress || app.CurrentState == Application.State.Change || app.CurrentState == Application.State.New)
                {
                    await ApplicationService.AskNextQuestion(Context, new List<Application.State>() { Application.State.InProgress, Application.State.Change, Application.State.New });
                    var response = await WaitForMessage(Context.User, Context.Channel, TimeSpan.Zero);
                    ValidAnswerResponsePrecondition preCon = new ValidAnswerResponsePrecondition(app, response);
                    ResponsePreconditionResult result = preCon.CheckPermissions().Result;
                    if (result.IsSuccess)
                    {
                        await app.AnswerQuestion(app.CurrentQuestionID + 1, response.Content);
                    }
                    else
                    {
                        await ReplyAsync(result.ErrorReason);
                    }
                    await Next();
                }
                else if (app.CurrentState == Application.State.Finished)
                {
                    await Review();
                }
        }

        [Command("Review", RunMode = RunMode.Async), Summary("Allows the user to view their application answers"), RequireDMChannel]
        public async Task Review()
        {
                await ApplicationService.ReviewApplication(Context);
        }

        [Command("Change", RunMode = RunMode.Async), Summary("Allows the user to modify their application answers"), RequireDMChannel]
        public async Task Change(int QuestionID)
        {
                Application app = ApplicationService.GetApplicationsForChannel(Context.Channel.Id, new List<Application.State> { Application.State.Review }).FirstOrDefault(x => x.UserID == Context.User.Id);
                app.CurrentState = Application.State.Change;
                app.CurrentQuestionID = QuestionID - 1;
                await app.SaveApplication(ApplicationService.saveDir);
                await Next();
        }

        [Command("Submit", RunMode = RunMode.Async), Summary("Submits the users application for review by the Guild"), RequireDMChannel]
        public async Task Submit()
        {
                await ApplicationService.SubmitApplication(Context);
        }

        [Command("Approve", RunMode = RunMode.Async), Summary("Approves the users application"), RequireElevatedUserPrecondition]
        public async Task Approve(string ApplicationID)
        {
            await ApplicationService.ApproveApplication(Context, ApplicationID);
        }
    }
}
