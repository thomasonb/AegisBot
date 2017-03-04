using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AegisBot.Interfaces;
using Discord;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;

namespace AegisBot.Implementations
{
    public class ServiceFactory
    {

        private static string saveDir = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent?.Parent?.Parent?.FullName + "\\Services";


        public static List<IAegisService> Services = new List<IAegisService>();

        public static List<IAegisService> GetServicesForCommand(string command)
        {
            return Services.Where(x => x.ContainsCommand(command)).ToList();
        }

        public static IAegisService GetService<T>() where T : IAegisService
        {
            return Services.FirstOrDefault(x => x.GetType() == typeof(T));
        }

        public static IAegisService GetService(string ServiceName)
        {
            return Services.FirstOrDefault(x => x.GetType().Name.ToLower() == ServiceName.ToLower());
        }

        public static void LoadService<T>(DiscordClient client) where T : IAegisService
        {
            string ServiceFile = Directory.GetFiles(saveDir).FirstOrDefault(x => x.Substring(0, x.IndexOf(".")).Contains(typeof(T).Name));
            IAegisService service;
            if (ServiceFile != null)
            {
                using (StreamReader sr = new StreamReader(ServiceFile))
                {
                   service = JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
                }
            }
            else
            {
                service = Activator.CreateInstance<T>();
            }
            if (!Services.Any(x => x.GetType() == typeof(T)))
            {
                Services.Add(service);
                service.SaveService();
            }
            (Services.First(x => x.GetType() == typeof(T)) as AegisService).Client = client;
        }
    }
}
