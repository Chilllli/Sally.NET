using Newtonsoft.Json;
using Sally.NET.Core;
using Sally.NET.Core.ApiReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sally.NET.Handler
{
    public class WeatherApiHandler : HttpRequestBase
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly Uri weatherUri = new("https://api.openweathermap.org");
        
        public WeatherApiHandler()
        {
            httpClient.BaseAddress = weatherUri;
        }

        /// <summary>
        /// The <c>Request2WeatherApiAsync</c> method creates an api call to the weather api.<br />
        /// The parameter can be optional.
        /// </summary>
        /// <param name="Location">Determine location of the request. </param>
        /// <returns>
        /// Returns a string with the resulting json
        /// </returns>
        /// <remarks>If the weather api key is not set in the config file, then this method won't work.</remarks>
        /// <example>
        /// <code>
        /// ApiRequestService.Request2WeatherApiAsync("Berlin");
        /// 
        /// Result:
        /// 
        /// "coord":
        /// {
        ///     "lon":13.41,
        ///     "lat":52.52
        /// },
        /// "weather":
        /// [
        ///     {
        ///         "id":800,
        ///         "main":"Clear",
        ///         "description":"clear sky",
        ///         "icon":"01n"
        ///     }
        /// ],
        /// "base":"stations",
        /// "main":
        /// {
        ///     "temp":14.67,
        ///     "feels_like":13.01,
        ///     "temp_min":13.33,
        ///     "temp_max":16.11,
        ///     "pressure":1012,
        ///     "humidity":82
        /// },
        /// etc...
        /// 
        /// </code>
        /// </example>
        public async Task<WeatherApi> Request2WeatherApiAsync(string apiKey, string location)
        {
            return JsonConvert.DeserializeObject<WeatherApi>(await (CreateHttpRequest(httpClient, $"/data/2.5/weather?q={HttpUtility.UrlEncode(location, Encoding.UTF8)}&appid={apiKey}&units=metric").Result).Content.ReadAsStringAsync());
        }

        public bool TryGetWeatherApi(string apiKey, string location, out WeatherApi weatherApi)
        {
            weatherApi = Request2WeatherApiAsync(apiKey, location).Result;
            return weatherApi.StatusCode == 200;
        }
        public bool TryGetCurrentTemperature(string apiKey, string location, out float temperature)
        {
            WeatherApi weatherApi = Request2WeatherApiAsync(apiKey, location).Result;
            temperature = weatherApi.Weather.Temperature;
            return weatherApi.StatusCode == 200;
        }

        public WeatherApi GetWeatherApiResult(string apiKey, string location)
        {
            return Request2WeatherApiAsync(apiKey, location).Result;
        }
    }
}
