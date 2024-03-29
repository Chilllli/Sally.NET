﻿using Discord.Commands;
using Discord.WebSocket;
using Discord;
using Sally.NET.Core;
using Sally.NET.DataAccess.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally_NET.Command.Profile
{
    public class ProfileTextCommands : ModuleBase
    {
        private readonly IDBAccess dbAccess;
        public ProfileTextCommands(IDBAccess dbAccess)
        {
            this.dbAccess = dbAccess;
        }
        [Command("mute")]
        public async Task MuteBot()
        {
            User user = dbAccess.GetUser(Context.Message.Author.Id);
            user.HasMuted = true;
            dbAccess.UpdateUser(user);
            await Context.Message.Channel.SendMessageAsync("The bot is muted now");
        }

        [Command("unmute")]
        public async Task UnmuteBot()
        {
            User user = dbAccess.GetUser(Context.Message.Author.Id);
            user.HasMuted = false;
            dbAccess.UpdateUser(user);
            await Context.Message.Channel.SendMessageAsync("The bot is unmuted now");
        }

        //Math.Floor(-50 * (15 * Math.Sprt(15) * Math.Pow(y, 2) - 60 * Math.Pow(y, 2) - 4))
        [Command("myxp")]
        public async Task LevelOverview()
        {
            //User myUser = CommandHandlerService.MessageAuthor;
            User myUser = dbAccess.GetUser(Context.User.Id);

            if (Context.Message.Channel is SocketGuildChannel guildChannel)
            {
                //message from guild
                EmbedBuilder lvlEmbed = new EmbedBuilder()
                    .WithAuthor($"To {Context.Message.Author}")
                    .WithTimestamp(DateTime.Now)
                    .WithTitle("Personal Level/Exp Overview")
                    .WithDescription("Check how much xp you miss for the next level up.")
                    .WithThumbnailUrl(Context.Message.Author.GetAvatarUrl())
                    .AddField("Current Global Level", myUser.GuildSpecificUser.Sum(x => x.Value.Level) / myUser.GuildSpecificUser.Count)
                    .AddField($"Current \"{guildChannel.Guild.Name}\" Level", myUser.GuildSpecificUser[guildChannel.Guild.Id].Level)
                .AddField("Xp needed until level up", (Math.Floor(-50 * (15 * Math.Sqrt(15) * Math.Pow(myUser.GuildSpecificUser[guildChannel.Guild.Id].Level + 1, 2) - 60 * Math.Pow(myUser.GuildSpecificUser[guildChannel.Guild.Id].Level + 1, 2) - 4))) - myUser.GuildSpecificUser[guildChannel.Guild.Id].Xp)
                    .WithColor(new Discord.Color((uint)Convert.ToInt32(myUser.EmbedColor, 16)))
                .WithFooter(Sally.NET.DataAccess.File.FileAccess.GENERIC_FOOTER, Sally.NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL);
                await Context.Message.Channel.SendMessageAsync(embed: lvlEmbed.Build());
                return;
            }
            else
            {
                //message from dm channel
                EmbedBuilder lvlEmbed = new EmbedBuilder()
                    .WithAuthor($"To {Context.Message.Author}")
                    .WithTimestamp(DateTime.Now)
                    .WithTitle("Personal Level/Exp Overview")
                    .WithDescription("Check how much xp you miss for the next level up.")
                    .WithThumbnailUrl(Context.Message.Author.GetAvatarUrl())
                    .AddField("Current Global Level", myUser.GuildSpecificUser.Sum(x => x.Value.Level) / myUser.GuildSpecificUser.Count)
                    .WithColor(new Discord.Color((uint)Convert.ToInt32(myUser.EmbedColor, 16)))
                .WithFooter(Sally.NET.DataAccess.File.FileAccess.GENERIC_FOOTER, Sally.NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL);
                await Context.Message.Channel.SendMessageAsync(embed: lvlEmbed.Build());
                return;
            }
        }

        //[Command("start")]
        //public async Task WelcomeRoles()
        //{
        //    bool hasHigherLvlRole = false;
        //    ulong levelRoleId = 0;
        //    SocketGuildUser newUser = Context.Message.Author as SocketGuildUser;
        //    ulong newComerRoleId = RoleManagerService.roleDictionary.GetValueOrDefault(0);
        //    foreach (KeyValuePair<int, ulong> entry in RoleManagerService.roleDictionary)
        //    {
        //        if (newUser.Roles.ToList().Find(r => r.Id == entry.Value) != null)
        //            levelRoleId = entry.Value;
        //        hasHigherLvlRole = true;
        //    }
        //    //check if user has newcomer role
        //    if (newUser.Roles.ToList().Find(r => r.Id == newComerRoleId) == null && !hasHigherLvlRole)
        //    {
        //        await newUser.AddRoleAsync(Program.MyGuild.Roles.ToList().Find(r => r.Id == newComerRoleId));
        //        await Context.Message.Channel.SendMessageAsync("Welcome to the Server!");
        //    }
        //    else
        //    {
        //        if (hasHigherLvlRole)
        //        {
        //            SocketRole levelRole = Program.MyGuild.Roles.ToList().Find(r => r.Id == levelRoleId);
        //            await Context.Message.Channel.SendMessageAsync($"You are already: {levelRole.Name}");
        //        }
        //        else
        //        {
        //            await Context.Message.Channel.SendMessageAsync("You already have this role or a higher one!");
        //        }
        //    }
        //}

        [Group("status")]
        public class StatusManagement : ModuleBase
        {
            private readonly IDBAccess dbAccess;
            public StatusManagement(IDBAccess dbAccess)
            {
                this.dbAccess = dbAccess;
            }
            [Command("isMuted")]
            public async Task ShowMuteStatus()
            {
                User user = dbAccess.GetUser(Context.Message.Author.Id);
                if (user.HasMuted)
                {
                    await Context.Message.Channel.SendMessageAsync($"The bot is currently muted for you.");
                }
                else
                {
                    await Context.Message.Channel.SendMessageAsync($"The bot is currently not muted for you.");
                }
            }
        }
    }
}
