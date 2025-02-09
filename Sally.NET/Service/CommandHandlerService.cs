using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Sally.NET.Core;
using Sally.NET.Core.ApiReference;
using Sally.NET.Core.Configuration;
using Sally.NET.Core.Enum;
using Sally.NET.DataAccess.Database;
using Sally.NET.Handler;
using Sally.NET.Module;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GroupAttribute = Discord.Commands.GroupAttribute;
using IResult = Discord.Commands.IResult;
using ModuleInfo = Discord.Interactions.ModuleInfo;

namespace Sally.NET.Service
{
    /// <summary>
    /// This class handles all command-related content.
    /// </summary>
    public class CommandHandlerService
    {
        private DiscordSocketClient client;
        private BotCredentials credentials;
        private CommandService commands = new CommandService();
        private InteractionService? interaction;
        public Dictionary<ulong, char> IdPrefixCollection { get; set; } = new Dictionary<ulong, char>();
        private List<Type> commandClasses = new List<Type>();
        private IServiceProvider? services;
        //public   User MessageAuthor { get; set; }
        private int requestCounter;
        public   int RequestCounter
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
        private bool HasCleverbotApiKey;
        private ILog logger;
        private IDBAccess dbAccess;

        private Helper helper;
        private readonly IServiceProvider provider;
        private readonly MusicModule musicModule;

        public CommandHandlerService(DiscordSocketClient client, BotCredentials credentials, ILog logger, IDBAccess dBAccess, Helper helper, IServiceProvider provider, ConfigManager configManager, MusicModule musicModule)
        {
            this.client = client;
            this.credentials = credentials;
            this.dbAccess = dBAccess;
            this.helper = helper;
            this.provider = provider;
            this.musicModule = musicModule;
            HasCleverbotApiKey = configManager.OptionalSettings.Contains("CleverApi");
            this.logger = logger;
            client.MessageReceived += CommandHandler;
            client.SlashCommandExecuted += Client_SlashCommandExecuted;
            client.ButtonExecuted += Client_ButtonExecuted;
        }

        public async Task Start(IServiceProvider services)
        {
            InteractionService interaction = new InteractionService(client);
            AppDomain appDomain = AppDomain.CurrentDomain;
            Assembly[] assemblies = appDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                await commands.AddModulesAsync(assembly, services);
                await interaction.AddModulesAsync(assembly, services);
                commandClasses.AddRange(assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ModuleBase)) && !t.IsAbstract).ToList());
            }
            this.interaction = interaction;
            this.services = services;
        }

        private async Task Client_ButtonExecuted(SocketMessageComponent arg)
        {
            SocketGuildChannel guildChannel = (SocketGuildChannel)arg.Channel;
            MusicPlayer musicPlayer = musicModule.GetPlayerByGuildId(guildChannel.Id);
            switch (arg.Data.CustomId)
            {
                case "btnPause":
                    await arg.DeferAsync();
                    await arg.Channel.SendMessageAsync("Pause");
                    await musicPlayer.Pause();
                    break;
                case "btnRepeat":
                    await arg.DeferAsync();
                    await arg.Channel.SendMessageAsync("Repeat");
                    await musicPlayer.Repeat();
                    break;
                case "btnPrevious":
                    await arg.DeferAsync();
                    await arg.Channel.SendMessageAsync("Previous");
                    await musicPlayer.PlayPreviousTrack();
                    break;
                case "btnSkip":
                    await arg.DeferAsync();
                    await arg.Channel.SendMessageAsync("Skip");
                    await musicPlayer.PlayNextTrack();
                    break;
                //case "LinkToMAL":
                //    await arg.DeferAsync();
                //    await arg.Channel.SendMessageAsync("asdasd");
                //    break;
                default:
                    break;
            }
        }

        private async Task Client_SlashCommandExecuted(SocketInteraction arg)
        {
            //MessageAuthor = dbAccess.GetUser(arg.User.Id);
            var context = new SocketInteractionContext(client, arg);
            await interaction.ExecuteCommandAsync(context, services);
        }

        /// <summary>
        /// "Parses" the user input and classifies it using <see cref="InputType"/>
        /// </summary>
        /// <param name="message">Raw input</param>
        /// <returns>Classified message including input</returns>
        private Input ClassifyAs(SocketUserMessage message)
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
        private async Task HandleMessage(Input input)
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

        private async Task HandleCommand(Input input)
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
                    //TODO: maybe initalize values on startup because they dont change and cache results
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
                            stringValue = helper.CalcLevenshteinDistance(resultCommand, copMessage);
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
                        stringValue = helper.CalcLevenshteinDistance(resultCommand, copMessage);
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
                commandResult = String.Join(Environment.NewLine, messageCompareValues.Where(d => d.Value == messageCompareValues.Values.Min()).Select(k => k.Key));
                //await context.Channel.SendMessageAsync($"Sorry, I dont know what you are saying ¯\\_(ツ)_/¯, but did you mean: {commandResult}");
                
                EmbedBuilder embed = (await helper.GetEmbedBuilderBase(context.User.Id))
                    .WithThumbnailUrl("https://sallynet.blob.core.windows.net/content/question.png")
                    .WithTitle("Sorry, I dont know what you are saying ¯\\_(ツ)_/¯")
                    .AddField("I have following commands, which might be correct", commandResult);
                await context.Channel.SendMessageAsync(embed: embed.Build());
                return;
            }

            //Error Handler
            if (!result.IsSuccess)
            {
                logger.Error(result.ErrorReason);
                await context.Channel.SendMessageAsync("Oh no... Something went wrong...");
            }
            logger.Info($"{context.Message.Content} from {context.Message.Author}");
        }

        private async Task HandleNaturalInput(Input input)
        {
            SocketUserMessage message = input.Message;

            if ((message.Channel as SocketDMChannel) != null && HasCleverbotApiKey)
            {
                //dm channel
                if (message.Author.Id == client.CurrentUser.Id)
                    return;
                //privat message
                RequestCounter++;
                CleverApi messageOutput = JsonConvert.DeserializeObject<CleverApi>(await services.GetRequiredService<CleverbotApiHandler>().Request2CleverBotApiAsync(message, credentials.CleverApi));
                await message.Channel.SendMessageAsync(messageOutput.Answer);
            }
        }

        private async Task CommandHandler(SocketMessage arg)
        {
            // Don't process the command if it was a System Message
            SocketUserMessage message = arg as SocketUserMessage;

            // Don't process the command if it is invalid
            if (message == null)
            {
                return;
            }
            if (arg.Author.IsBot)
            {
                return;
            }
            //MessageAuthor = dbAccess.GetUser(message.Author.Id);
            Input input = ClassifyAs(message);
            await HandleMessage(input);
        }
    }
}
