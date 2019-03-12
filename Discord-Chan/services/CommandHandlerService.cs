using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Chan.commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.services
{
    static class CommandHandlerService
    {
        private static CommandService commands;
        private static IServiceProvider services;

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
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(Program.Client.CurrentUser, ref argPos) || message.Content == PingCommand.PongMessage))
                return;
            // Create a Command Context
            SocketCommandContext context = new SocketCommandContext(Program.Client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            IResult result = await commands.ExecuteAsync(context, argPos, services);
            //Error Handler
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
