using Discord.WebSocket;
using Newtonsoft.Json;
using Sally.NET.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sally.NET.Core
{
    public static class AddonLoader
    {
        private static Dictionary<string, object> assemblyConfigPairs = new Dictionary<string, object>();

        /// <summary>
        /// load all plugins from dll's
        /// </summary>
        /// <param name="client"></param>
        public static void Load(DiscordSocketClient client)
        {
            loadAssemblies();
            initializeServices(client);
        }

        private static void loadAssemblies()
        {
            foreach (string file in Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "addons"), "*.dll"))
            {
                Assembly.LoadFile(file);
            }
        }

        private static void initializeServices(DiscordSocketClient client)
        {
            List<Type> types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => typeof(IService).IsAssignableFrom(t) && t.IsClass)).ToList();
            foreach (Type type in types)
            {
                IService service = Activator.CreateInstance(type) as IService;
                service.Initialize(client, LoadConfigFromFile(type.Assembly));
            }
        }

        private static object LoadConfigFromFile(Assembly assembly)
        {
            string dllName = Path.GetFileNameWithoutExtension(assembly.Location);
            if (assemblyConfigPairs.ContainsKey(dllName))
            {
                return assemblyConfigPairs[dllName];
            }
            string path = $"configs/{dllName}.json";
            if (!File.Exists(path))
            {
                return null;
            }
            assemblyConfigPairs.Add(dllName, JsonConvert.DeserializeObject(File.ReadAllText(path)));
            return assemblyConfigPairs[dllName];
        }

        private static void SaveConfigToFile(object config)
        {
            Assembly assembly = config.GetType().Assembly;
            string dllName = Path.GetFileNameWithoutExtension(assembly.Location);
            File.WriteAllText($"configs/{dllName}.json", JsonConvert.SerializeObject(config));
        }
    }
}
