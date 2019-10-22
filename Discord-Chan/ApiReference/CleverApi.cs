using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sally_NET.ApiReference
{
    class CleverApi
    {
        [JsonProperty("converation_id")]
        public string Id { get; set; }

        [JsonProperty("output")]
        public string Answer { get; set; }
    }
}
