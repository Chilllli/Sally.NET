
using Discord.Audio;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
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

namespace Sally.NET.Core
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
            this.Duration = video.Duration.GetValueOrDefault();
            this.LowResUrl = video.Thumbnails[0].Url;
            this.Url = video.Url;
            this.SongTitle = video.Title;
            this.Author = video.Author.ChannelTitle;
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
    public class MusicPlayer
    {
        private TaskCompletionSource<bool> taskCompletionSource = new();
        private CancellationTokenSource cancellationTokenSource = new();
        private List<VideoInfo> musicQueue = new();
        private VideoInfo currentVideoInfo;
        private int currentVideoIndex = -1;
        private bool isPaused = false;
        private bool isRepeating = false;
        private bool isSkipped = false;
        private string lastStatus = "Initialize...";
        private Stopwatch stopwatch = new();
        private IAudioClient audioClient;
        public MusicPlayer(IAudioClient audioClient)
        {
            this.audioClient = audioClient;
            Task.Run(() => Play());
        }

        public async Task<bool> AddTrack(string url)
        {
            YoutubeClient tubeClient = new YoutubeClient();
            Uri uri;
            if (!(Uri.TryCreate(url, UriKind.Absolute, out uri) && uri.Scheme == Uri.UriSchemeHttps))
            {
                //no valid url
                return false;
            };
            if (!uri.Host.Contains("youtube"))
            {
                //no youtube link
                return false;
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
            IStreamInfo streamInfo = streamingManifest.GetAudioOnlyStreams().First();
            string path = Path.Combine(Path.GetTempPath(), new StringBuilder(videoId).Append(".").Append(streamInfo.Container.Name).ToString());
            string taskPath = Path.Combine(Path.GetTempPath(), $"{videoId}.pcm");
            if (!File.Exists(path))
            {
                using (MemoryStream outputStream = new MemoryStream())
                {
                    try
                    {
                        await tubeClient.Videos.Streams.DownloadAsync(streamInfo, path);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("cant download video");
                    }
                    using (FileStream fileStream = new FileStream(path, FileMode.Open))
                    {
                        try
                        {
                            await outputStream.CopyToAsync(fileStream);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("cant write to file");
                        }

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
            lock (musicQueue)
            {
                musicQueue.Add(new VideoInfo(video, taskPath));
                File.WriteAllText("songQueue.json", JsonConvert.SerializeObject(musicQueue));
            }
            await Task.Run(() =>
            {
                isPaused = false;
            });
            return true;
        }

        public async Task Play()
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
                    lock (musicQueue)
                    {
                        audioCount = musicQueue.Count;
                    }
                    if (audioCount == currentVideoIndex + 1 && (currentVideoInfo.Path == null && !isRepeating))
                    {
                        //show nothing in media player
                        currentVideoInfo = default(VideoInfo);
                    }
                    else
                    {
                        if (!pause)
                        {
                            //Get Song

                            lock (musicQueue)
                            {
                                if (!(isRepeating && currentVideoInfo.Path != null))
                                {
                                    if (currentVideoIndex > 2)
                                    {
                                        musicQueue.RemoveAt(0);
                                        File.WriteAllText("songQueue.json", JsonConvert.SerializeObject(musicQueue));
                                    }
                                    else
                                    {
                                        currentVideoIndex++;
                                    }
                                }
                                currentVideoInfo = musicQueue[currentVideoIndex];
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
                            await sendAudio(currentVideoInfo.Path);
                            try
                            {
                                lock (musicQueue)
                                {
                                    if (musicQueue.Count(e => e.Path == currentVideoInfo.Path) == 0 && !isRepeating)
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

        public async Task Resume()
        {
            isPaused = false;
        }

        public async Task Pause()
        {
            isPaused = true;
        }

        public async Task Repeat()
        {
            isRepeating = true;
        }

        public async Task PlayPreviousTrack()
        {
            if (currentVideoIndex > 0)
            {
                currentVideoIndex -= 2;
                isSkipped = true;
            }
        }

        public async Task PlayNextTrack()
        {
            isSkipped = true;
        }

        private async Task sendAudio(string path)
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
                            !isSkipped &&                                    // If Skip is set to true, stop sending and set back to false (with getter)
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


                                if (isPaused)
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

        private string extractVideoIdFromUri(Uri uri)
        {
            const string YoutubeLinkRegex = "(?:.+?)?(?:\\/v\\/|watch\\/|\\?v=|\\&v=|youtu\\.be\\/|\\/v=|^youtu\\.be\\/)([a-zA-Z0-9_-]{11})+";
            Regex regexExtractId = new(YoutubeLinkRegex, RegexOptions.Compiled);
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
