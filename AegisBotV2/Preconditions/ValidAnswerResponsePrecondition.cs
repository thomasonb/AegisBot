using AegisBotV2.Implementations;
using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AegisBotV2.Preconditions
{
    class ValidAnswerResponsePrecondition
    {
        private Application _app;
        private IUserMessage _message;


        public ValidAnswerResponsePrecondition(Application app, IUserMessage message)
        {
            _app = app;
            _message = message;
        }

        public Task<ResponsePreconditionResult> CheckPermissions()
        {
            if (!_app.QAs[_app.CurrentQuestionID].ValidAnswers.Any())
            {
                return Task.FromResult(ResponsePreconditionResult.FromSuccess());
            }
            else
            {
                if (_app.QAs[_app.CurrentQuestionID].ValidAnswers.Any(x => x.ToLower() == _message.Content.ToLower()))
                {
                    return Task.FromResult(ResponsePreconditionResult.FromSuccess());
                }
            }
            return Task.FromResult(ResponsePreconditionResult.FromError($"{_message.Content} is not a Valid Answer"));
        }
    }
}
