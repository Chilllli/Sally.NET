using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Sally.NET.Core;
using Sally.NET.Core.ApiReference;
using Sally.NET.Core.Configuration;
using Sally.NET.Core.Enum;
using Sally.NET.DataAccess.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.Service
{
    /// <summary>
    /// This class handles all command-related content.
    /// </summary>
    public static class CommandHandlerService
    {
        private static DiscordSocketClient client;
        private static BotCredentials credentials;
        private static CommandService commands;
        public static Dictionary<ulong, char> IdPrefixCollection = new Dictionary<ulong, char>();
        private static List<Type> commandClasses;
        private static IServiceProvider services;
        public static User MessageAuthor { get; set; }
        private static int requestCounter;
        public static int RequestCounter
        {
            get
            {
                return requestCounter;
            }
            set
            {
                requestCounter = value;
                File.WriteAllText("ApiRequests.txt", requestCounter.ToString());
            }
        }
        private static bool HasCleverbotApiKey;

        public static async Task InitializeHandler(DiscordSocketClient client, BotCredentials credentials, List<Type> commandClasses, Dictionary<ulong, char> collection, bool hasCleverbotApiKey)
        {
            CommandHandlerService.client = client;
            CommandHandlerService.credentials = credentials;
            CommandHandlerService.commandClasses = commandClasses;
            CommandHandlerService.IdPrefixCollection = collection;
            HasCleverbotApiKey = hasCleverbotApiKey;
            commands = new CommandService();
            services = new ServiceCollection()
              .AddSingleton(client)
              .AddSingleton<InteractiveService>()
              .BuildServiceProvider();
            client.MessageReceived += CommandHandler;
            AppDomain appDomain = AppDomain.CurrentDomain;
            Assembly[] assemblies = appDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                await commands.AddModulesAsync(assembly, services);
            }

        }

        /// <summary>
        /// "Parses" the user input and classifies it using <see cref="InputType"/>
        /// </summary>
        /// <param name="message">Raw input</param>
        /// <returns>Classified message including input</returns>
        private static Input ClassifyAs(SocketUserMessage message)
        {
            char newPrefix;
            int argPos = 0;

            //check if the correct prefix is used for the specific guild
            //try casting channel to guild channel to aquire guild id
            if (message.Channel is SocketGuildChannel guildChannel)
            {
                ulong guildId = guildChannel.Guild.Id;
                if (!IdPrefixCollection.ContainsKey(guildId))
                {
                    //if dictonary doesn't contain guildid, create a new entry with "$" as default
                    IdPrefixCollection.TryAdd(guildId, '$');

                }
                //get custom prefix from dictronary
                newPrefix = IdPrefixCollection[guildId];
            }
            else
            {
                newPrefix = '$';
            }

            if (message.HasCharPrefix(newPrefix, ref argPos))
            {
                return new Input { Type = InputType.Command, Message = message, ArgumentPosition = argPos };
            }
            if (message.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                return new Input { Type = InputType.Mention, Message = message, ArgumentPosition = argPos };
            }

            // Fallback for all other types of input
            return new Input { Type = InputType.NaturalInput, Message = message };
        }

        /// <summary>
        /// Handles classified input (like responding to commands, etc.)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static async Task HandleMessage(Input input)
        {
            if (input.Type == InputType.Command || input.Type == InputType.Mention)
            {
                await HandleCommand(input);
                return;
            }

            if (input.Type == InputType.NaturalInput)
            {
                await HandleNaturalInput(input);
                return;
            }
        }

        private static async Task HandleCommand(Input input)
        {
            int argPos = input.ArgumentPosition ?? 0;

            SocketUserMessage message = input.Message;

            // Create a Command Context
            SocketCommandContext context = new SocketCommandContext(client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            IResult result = await commands.ExecuteAsync(context, argPos, services);
            string commandResult;
            if (result.Error == CommandError.UnknownCommand)
            {
                Dictionary<string, int> messageCompareValues = new Dictionary<string, int>();

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
                            stringValue = Helper.CalcLevenshteinDistance(resultCommand, copMessage);
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
                        stringValue = Helper.CalcLevenshteinDistance(resultCommand, copMessage);
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
                //await context.Channel.SendMessageAsync($"Sorry, I dont know what you are saying ¯\\_(ツ)_/¯, but did you mean: {commandResult}");
                EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(new Color((uint)Convert.ToInt32(CommandHandlerService.MessageAuthor.EmbedColor, 16)))
                    .WithCurrentTimestamp()
                    .WithFooter(NET.DataAccess.File.FileAccess.GENERIC_FOOTER, NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL)
                    .WithThumbnailUrl("https://sallynet.blob.core.windows.net/content/question.png")
                    .WithTitle("Sorry, I dont know what you are saying ¯\\_(ツ)_/¯")
                    .AddField("I have following commands, which might be correct", commandResult);
                await context.Channel.SendMessageAsync(embed: embed.Build());
                return;
            }

            //Error Handler
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync($"{result.ErrorReason} ¯\\_(ツ)_/¯");

            LoggerService.commandLogger.Log($"{context.Message.Content} from {context.Message.Author}");
        }

        private static async Task HandleNaturalInput(Input input)
        {
            SocketUserMessage message = input.Message;

            if ((message.Channel as SocketDMChannel) != null && HasCleverbotApiKey)
            {
                //dm channel
                if (message.Author.Id == client.CurrentUser.Id)
                    return;
                //privat message
                RequestCounter++;


                CleverApi messageOutput = JsonConvert.DeserializeObject<CleverApi>(await ApiRequestService.Request2CleverBotApiAsync(message));
                await message.Channel.SendMessageAsync(messageOutput.Answer);
            }
        }

        private static async Task CommandHandler(SocketMessage arg)
        {
            // Don't process the command if it was a System Message
            SocketUserMessage message = arg as SocketUserMessage;

            // Don't process the command if it is invalid
            if (message == null)
            {
                return;
            }

            //if ((client.Guilds.Where(g => g.Id == credentials.guildId).First()).Users.ToList().Find(u => u.Id == message.Author.Id) == null)
            //{
            //    return;
            //}
            if (arg.Author.IsBot)
            {
                return;
            }

            MessageAuthor = DatabaseAccess.Instance.Users.Find(u => u.Id == message.Author.Id);

            //await MessageHandlerService.DeleteStartMessages(message);
            Input input = ClassifyAs(message);
            await HandleMessage(input);
        }

        
    }
}
