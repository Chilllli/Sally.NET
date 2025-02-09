using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode.Videos;

namespace Sally.NET.Core
{
    public struct VideoInfo : IEquatable<VideoInfo>
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
}
