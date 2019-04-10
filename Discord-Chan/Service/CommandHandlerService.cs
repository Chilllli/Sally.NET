using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Chan.Command;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.Service
{
    static class CommandHandlerService
    {
        private static CommandService commands;
        private static IServiceProvider services;
        private static char prefix = '$';

        public static async Task InitializeHandler(DiscordSocketClient client)
        {
            commands = new CommandService();
            services = new ServiceCollection()
              .AddSingleton(Program.Client)
              .AddSingleton<InteractiveService>()
              .BuildServiceProvider();
            client.MessageReceived += CommandHandler;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        private static async Task CommandHandler(SocketMessage arg)
        {
            // Don't process the command if it was a System Message
            SocketUserMessage message = arg as SocketUserMessage;
            if (message == null)
                return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            //if (message.Author.Id == Program.Client.CurrentUser.Id)
            //    return;
            //if((message.Channel as SocketDMChannel) != null)
            //{
            //    //dm channel
            //    if(!message.HasCharPrefix(prefix, ref argPos))
            //    {
            //        //privat message
            //        Program.RequestCounter++;
            //    }
            //}
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix(prefix, ref argPos) || message.HasMentionPrefix(Program.Client.CurrentUser, ref argPos) || message.Content == PingCommand.PongMessage))
                return;
            // Create a Command Context
            SocketCommandContext context = new SocketCommandContext(Program.Client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            IResult result = await commands.ExecuteAsync(context, argPos, services);
            string commandResult;
            if (result.Error == CommandError.UnknownCommand)
            {
                Dictionary<string, int> messageCompareValues = new Dictionary<string, int>();
                List<Type> commandClasses = typeof(Discord_Chan.Command.PingCommand)
                    .Assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(ModuleBase)) && !t.IsAbstract).ToList();
                foreach (Type commandClass in commandClasses)
                {
                    MemberInfo[] memInfo = commandClass.GetMembers();
                    foreach (MemberInfo memberInfo in memInfo)
                    {
                        string resultCommand = "";
                        Object[] att = memberInfo.GetCustomAttributes(typeof(CommandAttribute), false);
                        if (att.Length == 0)
                            continue;
                        Type parent = commandClass;
                        while (parent != null)
                        {
                            Object[] classAtt = parent.GetCustomAttributes(typeof(GroupAttribute), false);
                            if (classAtt.Length != 0)
                            {
                                resultCommand = ((GroupAttribute)classAtt[0]).Prefix + " " + resultCommand;
                            }
                            parent = parent.DeclaringType;
                        }

                        resultCommand += ((CommandAttribute)att[0]).Text;
                        string copMessage = message.Content.Substring(1);
                        int stringValue;
                        while (copMessage.Contains(" "))
                        {
                            stringValue = CalcLevenshteinDistance(resultCommand, copMessage);
                            if (!messageCompareValues.ContainsKey(resultCommand))
                            {
                                messageCompareValues.Add(resultCommand, stringValue);
                            }
                            if (messageCompareValues[resultCommand] > stringValue)
                            {
                                messageCompareValues[resultCommand] = stringValue;
                            }
                            copMessage = copMessage.Substring(0, copMessage.LastIndexOf(" "));
                        }
                        stringValue = CalcLevenshteinDistance(resultCommand, copMessage);
                        if (!messageCompareValues.ContainsKey(resultCommand))
                        {
                            messageCompareValues.Add(resultCommand, stringValue);
                        }
                        if (messageCompareValues[resultCommand] > stringValue)
                        {
                            messageCompareValues[resultCommand] = stringValue;
                        }
                    }
                }

                int minValue = messageCompareValues.Values.Min();
                commandResult = String.Join(Environment.NewLine, messageCompareValues.Where(d => d.Value == minValue).Select(k => k.Key));
                await context.Channel.SendMessageAsync($"{result.ErrorReason} ¯\\_(ツ)_/¯, but did you mean: {commandResult}");
                return;
            }
            //Error Handler
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync($"{result.ErrorReason} ¯\\_(ツ)_/¯");
        }

        private static int CalcLevenshteinDistance(string a, string b)
        {
            if (String.IsNullOrEmpty(a) && String.IsNullOrEmpty(b))
            {
                return 0;
            }
            if (String.IsNullOrEmpty(a))
            {
                return b.Length;
            }
            if (String.IsNullOrEmpty(b))
            {
                return a.Length;
            }
            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++)
                ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++)
                ;

            for (int i = 1; i <= lengthA; i++)
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                    distances[i, j] = Math.Min
                        (
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost
                        );
                }
            return distances[lengthA, lengthB];
        }
    }
}
