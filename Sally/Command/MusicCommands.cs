using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using Sally.NET.Core;
using Sally.NET.Core.Configuration;
using Sally.NET.Module;
using Sally.NET.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Sally.Command
{
    public class MusicCommands : ModuleBase
    {
        private readonly MusicModule musicModule;

        public MusicCommands(MusicModule musicModule)
        {
            this.musicModule = musicModule;
        }

        [Command("testBtn")]
        public async Task TestBtn()
        {
            await Context.Message.Channel.SendMessageAsync("Button Test", components: musicModule.GetPlaylistEmbedButtons());
        }

        [Command("join", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task Join(IVoiceChannel? voiceChannel = null)
        {
            await musicModule.ClientJoinChannel(Context, voiceChannel);
        }

        //public static void Initialize(DiscordSocketClient client)
        //{
        //    Task.Run(() => playAudio());
        //    Task.Run(() => messageWatcher());
        //    if (File.Exists("songQueue.json"))
        //    {
        //        audioQueue = JsonConvert.DeserializeObject<List<VideoInfo>>(File.ReadAllText("songQueue.json"));
        //        if (audioQueue == null)
        //        {
        //            audioQueue = new List<VideoInfo>();
        //            File.WriteAllText("songQueue.json", JsonConvert.SerializeObject(audioQueue));
        //        }
        //    }
        //    else
        //    {
        //        audioQueue = new List<VideoInfo>();
        //        File.Create("songQueue.json").Close();
        //        File.WriteAllText("songQueue.json", JsonConvert.SerializeObject(audioQueue));
        //    }
        //    MusicCommands.client = client;
        //}

        //public static async void messageWatcher()
        //{
        //    while (true)
        //    {
        //        await Task.Delay(1000);
        //        if (Id == 0)
        //        {
        //            continue;
        //        }
        //        //SocketTextChannel textChannel = client.Guilds.First(g => g.GetChannel(Program.BotConfiguration.RadioControlChannel) != null).GetChannel(Program.BotConfiguration.RadioControlChannel) as SocketTextChannel;
        //        if (oneMessage == null)
        //        {
        //            continue;
        //        }
        //        var flattenPause = await (oneMessage.GetReactionUsersAsync(new Emoji("\u23EF"), 10000)).FlattenAsync();
        //        int currentPause = flattenPause.Count();
        //        var flattenPrevious = await (oneMessage.GetReactionUsersAsync(new Emoji("\u23EE"), 10000)).FlattenAsync();
        //        int currentPrevious = flattenPrevious.Count();
        //        var flattenSkip = await (oneMessage.GetReactionUsersAsync(new Emoji("\u23ED"), 10000)).FlattenAsync();
        //        int currentSkip = flattenSkip.Count();
        //        var flattenRepeat = await (oneMessage.GetReactionUsersAsync(new Emoji("\U0001F502"), 10000)).FlattenAsync();
        //        int currentRepeat = flattenRepeat.Count();

        //        if (currentPause + currentPrevious + currentRepeat + currentSkip == 0)
        //        {
        //            await oneMessage.AddReactionAsync(new Emoji("\u23EF"));
        //            await Task.Delay(1000);
        //            await oneMessage.AddReactionAsync(new Emoji("\u23EE"));
        //            await Task.Delay(1000);
        //            await oneMessage.AddReactionAsync(new Emoji("\u23ED"));
        //            await Task.Delay(1000);
        //            await oneMessage.AddReactionAsync(new Emoji("\U0001F502"));
        //            await Task.Delay(1000);
        //            lastPause = 1;
        //            lastPrevious = 1;
        //            lastRepeat = 1;
        //            lastSkip = 1;
        //            lock (lastStatus)
        //            {
        //                lastStatus = "Ready, use $add with url";
        //            }
        //            continue;
        //        }
        //        if (lastPause != currentPause)
        //        {
        //            if (currentVideoInfo.Path == null)
        //            {
        //                pause = false;
        //            }
        //            else
        //            {
        //                pause = !pause;
        //            }

        //            lock (lastStatus)
        //            {
        //                lastStatus = pause ? "Pause" : "Play";
        //            }
        //            await audioClient.SetSpeakingAsync(!pause);
        //        }
        //        if (lastSkip != currentSkip)
        //        {
        //            skip = true;
        //            lock (lastStatus)
        //            {
        //                lastStatus = "Skipping";
        //            }
        //        }
        //        if (lastRepeat != currentRepeat)
        //        {
        //            repeat = !repeat;
        //        }
        //        if (lastPrevious != currentPrevious)
        //        {
        //            if (currentVideoIndex > 0)
        //            {
        //                currentVideoIndex -= 2;
        //                skip = true;
        //                lock (lastStatus)
        //                {
        //                    lastStatus = "Previous Title";
        //                }

        //                //pause = false;
        //            }
        //        }

        //        lastPause = currentPause;
        //        lastPrevious = currentPrevious;
        //        lastSkip = currentSkip;
        //        lastRepeat = currentRepeat;

        //        await oneMessage.ModifyAsync(m => m.Embed = audioInfoEmbed());

        //    }
        //}
        //, RunMode = RunMode.Async
        [Command("add", RunMode = RunMode.Async)]
        public async Task AddTitle(string url)
        {
            await musicModule.AddSong(Context, url);
        }

        [Command("setMusicChannel")]
        public async Task SetMusicChannelForGuild(ulong channelId)
        {
            //change guildsetting class
            //add musicChannelId property
            
            await Task.CompletedTask;
        }

        [Command("add2")]
        public async Task AddTitleToPlayer(string url)
        {
            SocketGuildChannel guildChannel = (SocketGuildChannel)Context.Channel;
            MusicPlayer musicPlayer = musicModule.GetPlayerByGuildId(guildChannel.Id);
            if (musicPlayer == null)
            {
                //TODO: add better error message
                await Context.Channel.SendMessageAsync("hoops");
                return;
            }
            bool couldAdd = await musicPlayer.AddTrack(url);
            if (!couldAdd)
            {
                //TODO: add better error message
                await Context.Channel.SendMessageAsync("hoops");
                return;
            }
        }

        [Command("join2")]
        public async Task JoinVoiceChat(IVoiceChannel voiceChannel = null)
        {
            voiceChannel ??= (Context.Message.Author as IGuildUser)?.VoiceChannel;
            if (voiceChannel == null)
            {
                await Context.Message.Channel.SendMessageAsync("Please join a voice channel first.");
                return;
            }
            musicModule.AddPlayer(voiceChannel.GuildId, await voiceChannel.ConnectAsync());
        }
    }
}