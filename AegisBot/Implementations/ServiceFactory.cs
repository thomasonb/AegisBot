using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AegisBot.Interfaces;
using Discord;

namespace AegisBot.Implementations
{
    public class ServiceFactory
    {
        public static List<IAegisService> Services = new List<IAegisService>();

        public static List<IAegisService> GetServicesForCommand(string command)
        {
            return Services.Where(x => x.ContainsCommand(command)).ToList();
        }

        public static IAegisService GetService<T>() where T : IAegisService
        {
            return Services.FirstOrDefault(x => x.GetType() == typeof (T));
        }

        public static IAegisService GetService(string ServiceName)
        {
            return Services.FirstOrDefault(x => x.GetType().Name.ToLower() == ServiceName.ToLower());
        }

        public static void LoadService<T>(DiscordClient client) where T : IAegisService
        {
            Services.Add(Activator.CreateInstance<T>());
            (Services.First(x => x.GetType() == typeof(T)) as AegisService).Client = client;
        }
    }
}
