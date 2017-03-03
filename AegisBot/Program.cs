using Discord;
using AegisBot.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AegisBot.Interfaces;
namespace AegisBot
{
    class Program
    {
        private DiscordClient client;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Start();
        }

        public void Start()
        {
            client = new DiscordClient();
            LoadServices(client);
            HandleEvents(client);
        }

        public void LoadServices(DiscordClient client)
        {
            ServiceFactory.LoadService<ManagerService>(client);
            ServiceFactory.LoadService<ApplicationService>(client);
            ServiceFactory.LoadService<EchoService>(client);
        }

        public void HandleEvents(DiscordClient client)
        {
            ServiceFactory.Services.ForEach(x =>
            {
                x.HandleEvents();
            });

            client.ExecuteAndWait(async () =>
            {
                await client.Connect("myToken", TokenType.Bot);
            });
        }
    }
}
