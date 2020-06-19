using Discord;
using Discord.Addons.Interactive;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Sally.Command
{
    struct VideoInfo : IEquatable<VideoInfo>
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public TimeSpan Duration { get; set; }
        public string LowResUrl { get; set; }
        public string Url { get; set; }
        public string SongTitle { get; set; }
        public string Author { get; set; }
        public long Views { get; set; }

        public VideoInfo(Video video, string path)
        {
            this.Id = video.Id;
            this.Path = path;
            this.Duration = video.Duration;
            this.LowResUrl = video.Thumbnails.LowResUrl;
            this.Url = video.Url;
            this.SongTitle = video.Title;
            this.Author = video.Author;
            this.Views = video.Engagement.ViewCount;
        }
        public string Interpret
        {
            get
            {
                if (!SongTitle.Contains("-"))
                {
                    return Author;
                }
                return SongTitle.Substring(0, SongTitle.IndexOf("-")).Trim();
            }
        }
        public string Title
        {
            get
            {
                if (!SongTitle.Contains("-"))
                {
                    return SongTitle;
                }
                return SongTitle.Substring(SongTitle.IndexOf("-") + 1).Trim();
            }
        }

        public bool Equals(VideoInfo other)
        {
            return this.Id == other.Id;
        }
    }
    public static class TimeSpanExtension
    {
        public static string FormatTime(this TimeSpan timeSpan)
        {
            return ((timeSpan.Hours > 0) ? timeSpan.ToString("h\\:mm\\:ss") : timeSpan.ToString("mm\\:ss"));
        }
    }


    public class MusicCommands : InteractiveBase
    {
        private static Stopwatch stopwatch = new Stopwatch();
        public static void Initialize(DiscordSocketClient client)
        {
            Task.Run(() => playAudio());
            Task.Run(() => messageWatcher());
            if (File.Exists("songQueue.json"))
            {
                audioQueue = JsonConvert.DeserializeObject<List<VideoInfo>>(File.ReadAllText("songQueue.json"));
                if (audioQueue == null)
                {
                    audioQueue = new List<VideoInfo>();
                    File.WriteAllText("songQueue.json", JsonConvert.SerializeObject(audioQueue));
                }
            }
            else
            {
                audioQueue = new List<VideoInfo>();
                File.Create("songQueue.json");
                File.WriteAllText("songQueue.json", JsonConvert.SerializeObject(audioQueue));
            }
            MusicCommands.client = client;
        }

        private static IAudioClient audioClient;
        private static List<VideoInfo> audioQueue = new List<VideoInfo>();
        private static bool pause
        {
            get => _internalPause;
            set
            {
                Task.Run(() => taskCompletionSource.TrySetResult(value));
                _internalPause = value;
                if (value)
                {
                    stopwatch.Stop();
                }
                else if (!stopwatch.IsRunning)
                {
                    stopwatch.Start();
                }
            }
        }
        private static bool _internalPause;
        private static bool skip
        {
            get
            {
                bool ret = _internalSkip;
                _internalSkip = false;
                return ret;
            }
            set => _internalSkip = value;
        }
        private static bool _internalSkip = false;
        private static bool repeat = false;
        private static TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public static ulong Id { get; private set; }
        private static IUserMessage oneMessage = null;

        private static DiscordSocketClient client;
        private static VideoInfo currentVideoInfo;
        private static int currentVideoIndex = -1;

        [Command("join", RunMode = RunMode.Async)]
        public async Task Join(IVoiceChannel voiceChannel = null)
        {
            voiceChannel = voiceChannel ?? (Context.Message.Author as IGuildUser)?.VoiceChannel;
            if (voiceChannel == null)
            {
                await Context.Message.Channel.SendMessageAsync("You are not in a voice channel.");
                return;
            }
            SocketVoiceChannel lastVoiceChannel = Context.Client.Guilds.Select(g => g.VoiceChannels.Where(c => c.Users.Count(u => u.Id == Context.Client.CurrentUser.Id) > 0).FirstOrDefault()).Where(c => c != null).FirstOrDefault();
            if (lastVoiceChannel != null)
            {
                audioClient = await lastVoiceChannel.ConnectAsync();
                await audioClient.StopAsync();
            }
            audioClient = await voiceChannel.ConnectAsync();
            audioClient.SpeakingUpdated += AudioClient_SpeakingUpdated;

            //alle nachrichten löschen
            ITextChannel textChannel = (Context.Message.Channel as SocketGuildChannel).Guild.GetChannel(Program.BotConfiguration.radioControlChannel) as SocketTextChannel;
            List<IMessage> userMessages = await (textChannel.GetMessagesAsync().Flatten()).ToListAsync();
            foreach (IMessage message in userMessages)
            {
                await message.DeleteAsync();
            }


            oneMessage = await textChannel.SendMessageAsync(embed: audioInfoEmbed());
            Id = oneMessage.Id;
            await Context.Message.DeleteAsync();
        }

        private async Task AudioClient_SpeakingUpdated(ulong arg1, bool arg2)
        {
            //is speaking
        }

        private static int lastPause = 0;
        private static int lastPrevious = 0;
        private static int lastSkip = 0;
        private static int lastRepeat = 0;
        private static string lastStatus = "laden";

        public static async void messageWatcher()
        {
            while (true)
            {
                await Task.Delay(1000);
                if (Id == 0)
                {
                    continue;
                }
                SocketTextChannel textChannel = client.Guilds.First(g => g.GetChannel(Program.BotConfiguration.radioControlChannel) != null).GetChannel(Program.BotConfiguration.radioControlChannel) as SocketTextChannel;
                if (oneMessage == null)
                {
                    continue;
                }
                var flattenPause = await (oneMessage.GetReactionUsersAsync(new Emoji("\u23EF"), 10000)).FlattenAsync();
                int currentPause = flattenPause.Count();
                var flattenPrevious = await (oneMessage.GetReactionUsersAsync(new Emoji("\u23EE"), 10000)).FlattenAsync();
                int currentPrevious = flattenPrevious.Count();
                var flattenSkip = await (oneMessage.GetReactionUsersAsync(new Emoji("\u23ED"), 10000)).FlattenAsync();
                int currentSkip = flattenSkip.Count();
                var flattenRepeat = await (oneMessage.GetReactionUsersAsync(new Emoji("\U0001F502"), 10000)).FlattenAsync();
                int currentRepeat = flattenRepeat.Count();

                if (currentPause + currentPrevious + currentRepeat + currentSkip == 0)
                {
                    await oneMessage.AddReactionAsync(new Emoji("\u23EF"));
                    await Task.Delay(1000);
                    await oneMessage.AddReactionAsync(new Emoji("\u23EE"));
                    await Task.Delay(1000);
                    await oneMessage.AddReactionAsync(new Emoji("\u23ED"));
                    await Task.Delay(1000);
                    await oneMessage.AddReactionAsync(new Emoji("\U0001F502"));
                    await Task.Delay(1000);
                    lastPause = 1;
                    lastPrevious = 1;
                    lastRepeat = 1;
                    lastSkip = 1;
                    lock (lastStatus)
                    {
                        lastStatus = "Ready, use $add with url";
                    }
                    continue;
                }
                if (lastPause != currentPause)
                {
                    if (currentVideoInfo.Path == null)
                    {
                        pause = false;
                    }
                    else
                    {
                        pause = !pause;
                    }

                    lock (lastStatus)
                    {
                        lastStatus = pause ? "Pause" : "Play";
                    }
                    await audioClient.SetSpeakingAsync(!pause);
                }
                if (lastSkip != currentSkip)
                {
                    skip = true;
                    lock (lastStatus)
                    {
                        lastStatus = "Skipping";
                    }
                }
                if (lastRepeat != currentRepeat)
                {
                    repeat = !repeat;
                }
                if (lastPrevious != currentPrevious)
                {
                    if (currentVideoIndex > 0)
                    {
                        currentVideoIndex -= 2;
                        skip = true;
                        lock (lastStatus)
                        {
                            lastStatus = "Previous Title";
                        }

                        //pause = false;
                    }
                }

                lastPause = currentPause;
                lastPrevious = currentPrevious;
                lastSkip = currentSkip;
                lastRepeat = currentRepeat;

                await oneMessage.ModifyAsync(m => m.Embed = audioInfoEmbed());

            }
        }
        //, RunMode = RunMode.Async
        [Command("add", RunMode = RunMode.Async)]
        public async Task AddTitle(string url)
        {
            YoutubeClient tubeClient = new YoutubeClient();
            Uri uri;
            if (!(Uri.TryCreate(url, UriKind.Absolute, out uri) && uri.Scheme == Uri.UriSchemeHttps))
            {
                //no valid url
                await Context.Message.Channel.SendMessageAsync("no valid url");
                return;
            };
            if (!uri.Host.Contains("youtube"))
            {
                //no youtube link
                await Context.Message.Channel.SendMessageAsync("no youtube link");
                return;
            }
            //new Task(() =>
            // {
            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] - beginning downloading: {uri.ToString()}");
            lock (lastStatus)
            {
                lastStatus = "Begin download and convert";
            }
            string videoId = extractVideoIdFromUri(uri);
            Video video = await tubeClient.Videos.GetAsync(uri.ToString());
            StreamManifest streamingManifest = await tubeClient.Videos.Streams.GetManifestAsync(videoId);
            IStreamInfo streamInfo = streamingManifest.GetAudioOnly().WithHighestBitrate();
            string path = Path.Combine(Path.GetTempPath(), videoId + "." + streamInfo.Container.Name);
            string taskPath = Path.Combine(Path.GetTempPath(), $"{videoId}.pcm");
            if (!File.Exists(path))
            {
                using (MemoryStream outputStream = new MemoryStream())
                {
                    try
                    {
                        await tubeClient.Videos.Streams.DownloadAsync(streamInfo, path);
                    }
                    catch (Exception e)
                    {

                    }


                    try
                    {
                        using (FileStream fileStream = new FileStream(path, FileMode.Open))
                        {
                            try
                            {
                                await outputStream.CopyToAsync(fileStream);
                            }
                            catch (Exception e)
                            {

                            }

                        }

                    }
                    catch (Exception e)
                    {

                    }
                }

                Process.Start(new ProcessStartInfo()
                {
                    FileName = "ffmpeg",
                    Arguments = $"-xerror -i \"{path}\" -ac 2 -y -filter:a \"volume = 0.02100\" -loglevel panic -f s16le -ar 48000 \"{taskPath}\"",
                    UseShellExecute = false,    //TODO: true or false?
                    RedirectStandardOutput = false
                }).WaitForExit();

                File.Delete(path);
            }
            lock (audioQueue)
            {
                audioQueue.Add(new VideoInfo(video, taskPath));
                File.WriteAllText("songQueue.json", JsonConvert.SerializeObject(audioQueue));
            }
            await Task.Run(() =>
            {
                pause = false;
            });
            await Context.Message.DeleteAsync();
        }

        private static async void playAudio()
        {
            bool next = false;
            while (true)
            {
                bool pause = false;
                //Next song if current is over
                if (!next)
                {
                    pause = await taskCompletionSource.Task;
                    taskCompletionSource = new TaskCompletionSource<bool>();
                }
                else
                {
                    next = false;
                }

                try
                {
                    int audioCount = 0;
                    lock (audioQueue)
                    {
                        audioCount = audioQueue.Count;
                    }
                    if (audioCount == currentVideoIndex + 1 && (currentVideoInfo.Path == null && !repeat))
                    {
                        //show nothing in media player
                        currentVideoInfo = default(VideoInfo);
                    }
                    else
                    {
                        if (!pause)
                        {
                            //Get Song

                            lock (audioQueue)
                            {
                                if (!(repeat && currentVideoInfo.Path != null))
                                {
                                    if (currentVideoIndex > 2)
                                    {
                                        audioQueue.RemoveAt(0);
                                        File.WriteAllText("songQueue.json", JsonConvert.SerializeObject(audioQueue));
                                    }
                                    else
                                    {
                                        currentVideoIndex++;
                                    }
                                }
                                currentVideoInfo = audioQueue[currentVideoIndex];
                            }

                            stopwatch.Reset();
                            stopwatch.Start();
                            //Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] - begin playback: {currentVideoInfo.Title}");
                            //Update "Playing .."
                            //show what is currently playing

                            //Send audio (Long Async blocking, Read/Write stream)
                            lock (lastStatus)
                            {
                                lastStatus = "Playing";
                            }
                            await SendAudio(currentVideoInfo.Path);
                            try
                            {
                                lock (audioQueue)
                                {
                                    if (audioQueue.Count(e => e.Path == currentVideoInfo.Path) == 0 && !repeat)
                                    {
                                        File.Delete(currentVideoInfo.Path);
                                    }

                                }
                            }
                            catch
                            {
                                // ignored
                            }
                            finally
                            {
                                //Finally remove song from playlist

                            }
                            next = true;
                        }
                    }
                }
                catch (Exception)
                {
                    //audio can't be played

                }
            }
        }

        private static async Task SendAudio(string path)
        {
            await audioClient.SetSpeakingAsync(true);
            try
            {
                using (Stream output = File.Open(path, FileMode.Open))
                {
                    using (AudioOutStream discord = audioClient.CreatePCMStream(AudioApplication.Mixed))
                    {
                        //Adjust?
                        int bufferSize = 1024;
                        int bytesSent = 0;
                        bool fail = false;
                        bool exit = false;
                        byte[] buffer = new byte[bufferSize];

                        while (
                            !skip &&                                    // If Skip is set to true, stop sending and set back to false (with getter)
                            !fail &&                                    // After a failed attempt, stop sending
                            !cancellationTokenSource.IsCancellationRequested &&   // On Cancel/Dispose requested, stop sending
                            !exit                                       // Audio Playback has ended (No more data from FFmpeg.exe)
                                )
                        {
                            try
                            {
                                int read = await output.ReadAsync(buffer, 0, bufferSize, cancellationTokenSource.Token);
                                if (read == 0)
                                {
                                    //No more data available
                                    //Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] - End of song: {currentVideoInfo.Title}");
                                    exit = true;
                                    break;
                                }

                                await discord.WriteAsync(buffer, 0, read, cancellationTokenSource.Token);


                                if (pause)
                                {
                                    bool pauseAgain;
                                    do
                                    {
                                        pauseAgain = await taskCompletionSource.Task;
                                        taskCompletionSource = new TaskCompletionSource<bool>();
                                    } while (pauseAgain);
                                }

                                bytesSent += read;

                            }
                            catch (TaskCanceledException)
                            {
                                exit = true;
                            }
                            catch (Exception)
                            {
                                fail = true;
                                //Console.WriteLine(e.Message);
                                // could not send
                            }
                        }
                        await discord.FlushAsync();
                        await audioClient.SetSpeakingAsync(false);
                    }
                }

            }
            catch (Exception)
            {
                //Console.WriteLine(e);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static Embed audioInfoEmbed()
        {
            EmbedBuilder dynamicEmbed = new EmbedBuilder();
            if (currentVideoInfo.Path == null)
            {
                dynamicEmbed
                .WithDescription("Use `$add url` to add music to the queue")
                .WithAuthor("Welcome to the music player")
                .WithColor(Color.DarkBlue)
                .WithTitle("♪♫♪ Media Player ♫♪♫")
                .WithFooter(f => f.Text = "stop \u23EF, skip \u23ED, previous \u23EE, repeat \U0001F502")
                .Build();
            }
            else
            {
                dynamicEmbed
                .WithAuthor($"{currentVideoInfo.Title}")
                .WithColor(Color.DarkPurple)
                .WithDescription($"`|{"".PadLeft((int)(40.0 * (stopwatch.Elapsed.TotalSeconds / currentVideoInfo.Duration.TotalSeconds)), '#').PadRight(40, '-')}|`\n\nZeit: {stopwatch.Elapsed.FormatTime()} / {currentVideoInfo.Duration.FormatTime()}")
                .WithFooter(f => f.Text = "stop \u23EF, skip \u23ED, previous \u23EE, repeat \U0001F502")
                .WithTitle($"{currentVideoInfo.Interpret}\n\n")
                .WithUrl(currentVideoInfo.Url)
                .WithThumbnailUrl(currentVideoInfo.LowResUrl);


            }
            lock (audioQueue)
            {
                foreach (VideoInfo song in audioQueue)
                {
                    if (song.Equals(currentVideoInfo))
                    {
                        dynamicEmbed.AddField($"{(pause ? ":pause_button:" : ":arrow_forward:")}  {(repeat ? "\U0001F502" : "")}" + song.Title, song.Duration.FormatTime());
                    }
                    else
                    {
                        dynamicEmbed.AddField(song.Title, song.Duration.FormatTime() + $" https://youtu.be/{song.Id}");
                    }

                }
                lock (lastStatus)
                {
                    dynamicEmbed.AddField("Status:", lastStatus);
                }
            }
            return dynamicEmbed.Build();
        }

        [Command("setMusicChannel")]
        public async Task SetMusicChannelForGuild(ulong channelId)
        {
            //change guildsetting class
            //add musicChannelId property
            
            await Task.CompletedTask;
        }


        private string extractVideoIdFromUri(Uri uri)
        {
            const string YoutubeLinkRegex = "(?:.+?)?(?:\\/v\\/|watch\\/|\\?v=|\\&v=|youtu\\.be\\/|\\/v=|^youtu\\.be\\/)([a-zA-Z0-9_-]{11})+";
            Regex regexExtractId = new Regex(YoutubeLinkRegex, RegexOptions.Compiled);
            string[] validAuthorities = { "youtube.com", "www.youtube.com", "youtu.be", "www.youtu.be" };
            try
            {
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
            }
            catch { }


            return null;
        }
    }
}