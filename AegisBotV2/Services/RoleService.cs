using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using AegisBotV2.Implementations;
using AegisBotV2.Preconditions;
using Discord.Addons.InteractiveCommands;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace AegisBotV2.Services
{
    public static class RoleService
    {
        internal static async Task SetRank(CommandContext context, string rankName)
        {
            Application app = ApplicationService.GetApplicationByUser(context.User.Id);
            if (app != null && app.CurrentState == Application.State.Approved)
            {
                ValidAnswerResponsePrecondition preCon = new ValidAnswerResponsePrecondition(app, context.Message);
                QA currentQuestion = app.QAs.FirstOrDefault(x => x.RoleType == QuestionRoleType.Rank);
                app.CurrentQuestionID = currentQuestion.QuestionID - 1;
                ResponsePreconditionResult result = preCon.CheckPermissions("SetRank").Result;
                if (result.IsSuccess)
                {
                    await (context.User as IGuildUser).RemoveRoleAsync(context.Guild.Roles.First(x => x.Name.ToLower() == currentQuestion.Answer.ToLower()));
                    await app.AnswerQuestion(app.CurrentQuestionID + 1, rankName);
                    app.CurrentQuestionID = app.QAs.Last().QuestionID - 1;
                    await (context.User as IGuildUser).AddRoleAsync(context.Guild.Roles.First(x => x.Name.ToLower() == rankName.ToLower()));
                }
            }
        }

        internal static async Task SetPlatform(CommandContext context, string platformName)
        {
            Application app = ApplicationService.GetApplicationByUser(context.User.Id);
            if (app != null && app.CurrentState == Application.State.Approved)
            {
                ValidAnswerResponsePrecondition preCon = new ValidAnswerResponsePrecondition(app, context.Message);
                QA currentQuestion = app.QAs.FirstOrDefault(x => x.RoleType == QuestionRoleType.Platform);
                app.CurrentQuestionID = currentQuestion.QuestionID - 1;
                ResponsePreconditionResult result = preCon.CheckPermissions("SetPlatform").Result;
                if (result.IsSuccess)
                {
                    await (context.User as IGuildUser).RemoveRoleAsync(context.Guild.Roles.First(x => x.Name.ToLower() == currentQuestion.Answer.ToLower()));
                    await app.AnswerQuestion(app.CurrentQuestionID + 1, platformName);
                    app.CurrentQuestionID = app.QAs.Last().QuestionID - 1;
                    await (context.User as IGuildUser).AddRoleAsync(context.Guild.Roles.First(x => x.Name.ToLower() == platformName.ToLower()));
                }
            }
        }

        internal static async Task SetRegion(CommandContext context, string regionName)
        {
            Application app = ApplicationService.GetApplicationByUser(context.User.Id);
            if (app != null && app.CurrentState == Application.State.Approved)
            {
                ValidAnswerResponsePrecondition preCon = new ValidAnswerResponsePrecondition(app, context.Message);
                QA currentQuestion = app.QAs.FirstOrDefault(x => x.RoleType == QuestionRoleType.Region);
                app.CurrentQuestionID = currentQuestion.QuestionID - 1;
                ResponsePreconditionResult result = preCon.CheckPermissions("SetRegion").Result;
                if (result.IsSuccess)
                {
                    await (context.User as IGuildUser).RemoveRoleAsync(context.Guild.Roles.First(x => x.Name.ToLower() == currentQuestion.Answer.ToLower()));
                    await app.AnswerQuestion(app.CurrentQuestionID + 1, regionName);
                    app.CurrentQuestionID = app.QAs.Last().QuestionID - 1;
                    await (context.User as IGuildUser).AddRoleAsync(context.Guild.Roles.First(x => x.Name.ToLower() == regionName.ToLower()));
                }
            }
        }
    }
}
