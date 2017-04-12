using Discord;
using AegisBot.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AegisBot.Interfaces;
using System.IO;
using System.Reflection;

namespace AegisBot
{
    class Program
    {
        private DiscordClient client;

        private string saveDir = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent?.Parent?.Parent?.FullName;


        static void Main(string[] args)
        {
            Program p = new Program();
            p.Start();
        }

        public void Start()
        {
            Console.WriteLine(saveDir);
            client = new DiscordClient();
            LoadServices(client);
            HandleEvents(client);
        }

        public void LoadServices(DiscordClient client)
        {
            ServiceFactory.LoadService<ManagerService>(client);
            ServiceFactory.LoadService<ApplicationService>(client);
            ServiceFactory.LoadService<EchoService>(client);
            Application app = new Application(0);
            app.LoadQuestions();
            ServiceFactory.GetService<ApplicationService>().LoadCommands();
        }

        public void HandleEvents(DiscordClient client)
        {
            ServiceFactory.Services.ForEach(x =>
            {
                x.HandleEvents();
            });

            client.ExecuteAndWait(async () =>
            {
                await client.Connect(GetAuthToken(), TokenType.Bot);
            });

            var y = client.Servers.First();
        }

        public string GetAuthToken()
        {
            string AuthFile = Directory.GetFiles(saveDir).FirstOrDefault(x => x.Contains("Auth.txt"));

            string token;
            using (StreamReader sr = new StreamReader(saveDir + "\\Auth.txt"))
            {
                token = sr.ReadToEnd();
            }
            return token;
        }
    }
}
