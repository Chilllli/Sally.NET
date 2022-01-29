using System;
using System.Collections.Generic;
using System.Text;

namespace Sally.NET.Core.Configuration
{
    public class ConfigManager
    {
        public List<string> OptionalSettings { get; set; } = new List<string>();
        public ConfigManager(BotCredentials credentials)
        {
            foreach (var property in credentials.GetType().GetProperties())
            {
                if (String.IsNullOrWhiteSpace(property.GetValue(credentials, null).ToString()))
                {
                    OptionalSettings.Add(property.Name);
                }
            }
        }
    }
}
