using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using log4net.Core;
using Mysqlx.Session;
using Newtonsoft.Json;
using Sally.NET.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;
using YoutubeExplode;
using System.Text.RegularExpressions;
using Sally.NET.Core.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Sally.NET.Module
{
    public class MusicModule
    {
        private ConcurrentDictionary<ulong, MusicPlayer> activeMusicPlayers = new();
        private readonly ILog logger;
        private readonly BotCredentials credentials;

        public MusicModule(ILog logger, BotCredentials credentials)
        {
            this.logger = logger;
            this.credentials = credentials;
        }

        public MusicPlayer? GetPlayerByGuildId(ulong guildId)
        {
            if (activeMusicPlayers.TryGetValue(guildId, out MusicPlayer musicPlayer))
            {
                return musicPlayer;
            }
            return null;
        }

        public MusicPlayer AddPlayer(ulong guildId, IAudioClient audioClient)
        {
            MusicPlayer player = new(audioClient);
            activeMusicPlayers.TryAdd(guildId, player);
            return player;
        }
        public MessageComponent GetPlaylistEmbedButtons()
        {
            return new ComponentBuilder()
                .WithButton("Pause", "btnPause", emote: new Emoji("\u23EF"))
                .WithButton("Previous", "btnPrevious", emote: new Emoji("\u23EE"))
                .WithButton("Skip", "btnSkip", emote: new Emoji("\u23ED"))
                .WithButton("Repeat", "btnRepeat", emote: new Emoji("\U0001F502"))
                .Build();
        }

        public async Task ClientJoinChannel(ICommandContext context, IVoiceChannel? voiceChannel = null)
        {
            MusicPlayer? musicPlayer;
            voiceChannel ??= (context.Message.Author as IGuildUser)?.VoiceChannel;
            if (voiceChannel == null)
            {
                await context.Message.Channel.SendMessageAsync("Please join a voice channel first.");
                return;
            }
            try
            {
                var audioClient = await voiceChannel.ConnectAsync();
                musicPlayer = AddPlayer((context.Message.Channel as IGuildChannel).GuildId, audioClient);
            }
            catch (Exception ex)
            {
                logger.Error("Error", ex);
                throw;
            }
            //handle null textChannel
            //temporarily disable delete messages
            ITextChannel textChannel = (context.Message.Channel as SocketGuildChannel).Guild.GetChannel(credentials.RadioControlChannel) as SocketTextChannel;
            textChannel ??= context.Channel as ITextChannel;
            //List<IMessage> userMessages = await (textChannel.GetMessagesAsync().Flatten()).ToListAsync();
            //foreach (IMessage message in userMessages)
            //{
            //    await message.DeleteAsync();
            ////}
            await textChannel.SendMessageAsync(embed: musicPlayer.AudioInfoEmbed());
            //Id = oneMessage.Id;
            //await context.Message.DeleteAsync();
        }

        public async Task AddSong(ICommandContext context, string url)
        {
            MusicPlayer? musicPlayer = GetPlayerByGuildId((context.Channel as IGuildChannel).GuildId);
            if (musicPlayer == null)
            {
                await ClientJoinChannel(context);
                musicPlayer = GetPlayerByGuildId((context.Channel as IGuildChannel).GuildId);
            }
            await musicPlayer!.AddTrack(url);
        }

        private string extractVideoIdFromUri(Uri uri)
        {
            const string YoutubeLinkRegex = "(?:.+?)?(?:\\/v\\/|watch\\/|\\?v=|\\&v=|youtu\\.be\\/|\\/v=|^youtu\\.be\\/)([a-zA-Z0-9_-]{11})+";
            Regex regexExtractId = new Regex(YoutubeLinkRegex, RegexOptions.Compiled);
            string[] validAuthorities = { "youtube.com", "www.youtube.com", "youtu.be", "www.youtu.be" };
            string authority = new UriBuilder(uri).Uri.Authority.ToLower();

            //check if the url is a youtube url
            if (validAuthorities.Contains(authority))
            {
                //and extract the id
                var regRes = regexExtractId.Match(uri.ToString());
                if (regRes.Success)
                {
                    return regRes.Groups[1].Value;
                }
            }
            return null;
        }
    }
}
