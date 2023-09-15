using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Sally.NET.Core;
using Sally.NET.DataAccess.Database;
using Sally.NET.Module;
using Sally.NET.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sally_NET.Command
{
    public class GuildCommands : ModuleBase
    {
        /// <summary>
        /// command group for setting or updating something on a guild
        /// </summary>
        [Group("set")]
        public class SetCommands : ModuleBase
        {
            private readonly IDBAccess dbAccess;
            public SetCommands(IDBAccess dbAccess)
            {
                this.dbAccess = dbAccess;
            }
            /// <summary>
            /// sets the channel for the music playlist embed
            /// </summary>
            /// <param name="musicChannelId"></param>
            /// <returns></returns>
            [Command("musicchannel")]
            public async Task SetMusicChannelForGuild(ulong musicChannelId)
            {
                //check channel of message
                if (Context.Message.Channel is SocketGuildChannel guildChannel)
                {
                    SocketGuild guild = guildChannel.Guild;
                    //check if channel is on server
                    if (guild.GetTextChannel(musicChannelId) == null)
                    {
                        await Context.Message.Channel.SendMessageAsync("Cannot find channel on the server.");
                        return;
                    }
                    //add or update channel to guildsettings from server
                    GuildSettings? guildSetting = await dbAccess.GetGuildSettingsByIdAsync(guild.Id);
                    if (guildSetting == null)
                    {
                        await Context.Message.Channel.SendMessageAsync($"No guild settings found.");
                        return;
                    }
                    guildSetting.MusicChannelId = musicChannelId;
                    dbAccess.UpdateGuildSettings(guildSetting);
                    await Context.Message.Channel.SendMessageAsync($"{guildChannel.Name} was set as music channel.");
                }
                else
                {
                    //message wasn't send from a server
                    await Context.Message.Channel.SendMessageAsync("Try this command on a server.");
                }
            }

            /// <summary>
            /// sets a custom prefix
            /// </summary>
            /// <param name="prefix"></param>
            /// <returns></returns>
            [Command("prefix")]
            public async Task ChangePrefix(char prefix)
            {
                //try casting to guild channel
                if (Context.Message.Channel is SocketGuildChannel guildChannel)
                {
                    //check if this user is an admin of the specific guild
                    SocketGuildUser user = guildChannel.Guild.Users.ToList().Find(u => u.Id == Context.Message.Author.Id);
                    if (AdminModule.IsAuthorized(user))
                    {
                        CommandHandlerService.IdPrefixCollection[guildChannel.Guild.Id] = prefix;
                        File.WriteAllText("meta/prefix.json", JsonConvert.SerializeObject(CommandHandlerService.IdPrefixCollection));
                        await Context.Message.Channel.SendMessageAsync($"Now the new prefix is \"{prefix}\"");
                    }
                    else
                    {
                        await Context.Message.Channel.SendMessageAsync("You have no permission.");
                    }
                }
                else
                {
                    //channel isn't a guild channel
                    await Context.Message.Channel.SendMessageAsync("This command has no effect here. Try using it on a guild.");
                }
            }
        }

        /// <summary>
        /// command group for adding something and keep existing values on a guild
        /// </summary>
        [Group("add")]
        public class AddCommands : ModuleBase
        {
            [Command("role")]
            public async Task AddRankRole(int index, ulong roleId)
            {
                if (Context.Message.Channel is SocketGuildChannel guildChannel)
                {
                    //get user from guild
                    SocketGuildUser user = guildChannel.Guild.Users.ToList().Find(u => u.Id == Context.Message.Author.Id);
                    //check guild priviliges
                    if (!AdminModule.IsAuthorized(user))
                    {
                        await Context.Message.Channel.SendMessageAsync("You have no permission for this command.");
                        return;
                    }
                    ulong guildId = guildChannel.Guild.Id;
                    SocketGuild guild = guildChannel.Guild;
                    Dictionary<int, ulong>  guildRankRoleCollection = RoleManagerService.RankRoleCollection[guildId];
                    if (index > 500)
                    {
                        await Context.Message.Channel.SendMessageAsync("Desired level is too high.");
                        return;
                    }
                    if (guild.Roles.ToList().Find(r => r.Id == roleId) == null)
                    {
                        await Context.Message.Channel.SendMessageAsync("There is no role with this id on this guild.");
                        return;
                    }
                    if (guildRankRoleCollection.Count >= 20)
                    {
                        await Context.Message.Channel.SendMessageAsync("You can't add anymore roles.");
                        return;
                    }
                    if (guildRankRoleCollection.Values.ToList().Contains(roleId))
                    {
                        await Context.Message.Channel.SendMessageAsync("You have already added this role.");
                        return;
                    }
                    if (!guildRankRoleCollection.ContainsKey(index))
                    {
                        guildRankRoleCollection.Add(index, roleId);
                    }
                    guildRankRoleCollection[index] = roleId;
                    RoleManagerService.RankRoleCollection[guildId] = guildRankRoleCollection;
                    RoleManagerService.SaveRankRoleCollection();
                    await Context.Message.Channel.SendMessageAsync($"{guild.Roles.ToList().Find(r => r.Id == roleId).Name} was added with Level {index}.");
                }
                else
                {
                    await Context.Message.Channel.SendMessageAsync("You can only perfom this command on a server.");
                }
            }
        }

        /// <summary>
        /// command group for removing values from a guild
        /// </summary>
        [Group("remove")]
        public class RemoveCommands : ModuleBase
        {
            private readonly GeneralModule generalModule;
            public RemoveCommands(GeneralModule generalModule)
            {
                this.generalModule = generalModule;
            }
            [Command("role")]
            public async Task RemoveRankRole(int index)
            {
                if (Context.Message.Channel is SocketGuildChannel guildChannel)
                {
                    //check if user has admin priviliges
                    if (AdminModule.IsAuthorized(generalModule.GetGuildUserFromGuild(Context.Message.Author as SocketUser, guildChannel.Guild)))
                    {
                        await Context.Message.Channel.SendMessageAsync("You cant do this.");
                        return;
                    }
                    ulong guildId = guildChannel.Guild.Id;
                    SocketGuild guild = guildChannel.Guild;
                    Dictionary<int, ulong> guildRankRoleCollection = RoleManagerService.RankRoleCollection[guildId];
                    if (guildRankRoleCollection == null)
                    {
                        await Context.Message.Channel.SendMessageAsync("There are no roles to remove.");
                        return;
                    }
                    //if (guild.Roles.ToList().Find(r => r.Id == roleId) == null)
                    //{
                    //    await Context.Message.Channel.SendMessageAsync("This role dont exist on this server.");
                    //    return;
                    //}
                    //if (guildRankRoleCollection.Values.ToList().Contains(roleId))
                    //{
                    //    await Context.Message.Channel.SendMessageAsync("You cant remove this role, because it was never added.");
                    //    return;
                    //}
                    if (!guildRankRoleCollection.ContainsKey(index))
                    {
                        await Context.Message.Channel.SendMessageAsync("The current index is not in your role collection.");
                        return;
                    }
                    guildRankRoleCollection.Remove(index);
                    RoleManagerService.SaveRankRoleCollection();
                    await Context.Message.Channel.SendMessageAsync("Role successfully removed.");
                }
                else
                {
                    await Context.Message.Channel.SendMessageAsync("This is a server command only.");
                }
            }
        }

        /// <summary>
        /// command group for showing values on a guild
        /// </summary>
        [Group("show")]
        public class ShowCommands : ModuleBase
        {
            [Command("roles")]
            public async Task ShowRankRoles()
            {
                if (Context.Message.Channel is SocketGuildChannel guildChannel)
                {
                    ulong guildId = guildChannel.Guild.Id;
                    SocketGuild guild = guildChannel.Guild;
                    Dictionary<int, ulong> guildRankRoleCollection = RoleManagerService.RankRoleCollection[guildId];
                    if (guildRankRoleCollection.Count == 0)
                    {
                        await Context.Message.Channel.SendMessageAsync("You didn't added rank roles yet.");
                        return;
                    }
                    EmbedBuilder embed = new EmbedBuilder()
                        .WithTitle("Current Rank Roles")
                        .WithFooter(Sally.NET.DataAccess.File.FileAccess.GENERIC_FOOTER, Sally.NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL)
                        .WithTimestamp(DateTime.Now);

                    foreach (KeyValuePair<int, ulong> entry in guildRankRoleCollection)
                    {
                        SocketRole rankRole = guild.Roles.ToList().Find(r => r.Id == entry.Value);
                        embed.AddField(entry.Key.ToString(), rankRole.Name);
                    }
                    await Context.Message.Channel.SendMessageAsync(embed: embed.Build());
                }
                else
                {
                    await Context.Message.Channel.SendMessageAsync("You can only perfom this command on a server.");
                }
            }
        }
    }
}
