using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.Core.ApiReference
{
    public class WeatherApi
    {
        [JsonProperty("coord")]
        public Coordinate Coordinate { get; set; }
        [JsonProperty("main")]
        public Weather Weather { get; set; }
        [JsonProperty("wind")]
        public Wind Wind { get; set; }
        [JsonProperty("timezone")]
        public int Timezone { get; set; }
        [JsonProperty("name")]
        public string Location { get; set; }
        [JsonProperty("clouds")]
        public Cloud Clouds { get; set; }
        [JsonProperty("weather")]
        public WeatherCondition[] WeatherCondition { get; set; }
        [JsonProperty("cod")]
        public int StatusCode { get; set; }
    }

    public class WeatherCondition
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("main")]
        public string ShortDescription { get; set; }
        [JsonProperty("description")]
        public string LongDescription { get; set; }
    }

    public class Coordinate
    {
        public float Lon { get; set; }
        public float Lat { get; set; }
    }

    public class Weather
    {
        public float Temperature { get; set; }
        public float FeltTemperature { get; set; }
        public float MinTemperature { get; set; }
        public float MaxTemperature { get; set; }
        public int AirPressure { get; set; }
        public int Humidity { get; set; }
    }

    public class Wind
    {
        public float Speed { get; set; }
        public int Degree { get; set; }
        public float Gust {  get; set; }
    }

    
    public class Cloud
    {
        [JsonProperty("all")]
        public float Density { get; set; }
    }
}
