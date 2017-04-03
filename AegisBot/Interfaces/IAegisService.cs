using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AegisBot.Interfaces
{
    public interface IAegisService
    {
        bool ContainsCommand(string command);
        void HandleEvents();
        Task RunCommand(MessageEventArgs e);
        Task RunCommand(UserEventArgs e, string command);
        void SaveService();
        void LoadCommands();
    }
}
