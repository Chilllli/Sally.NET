using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sally_NET.ApiReference
{
    public class KonachanApi
    {
        private string[] tags { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("tags")]
        public string TagString { get; set; }
        public string[] Tags {
            get
            {
                return TagString.Split(" ");
            }
            set
            {

            }
        }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("jpeg_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("rating")]
        public char Rating { get; set; }
    }
}
