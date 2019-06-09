using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sally_NET.Service
{
    static class ApiRequestService
    {
        public static string StartRequest(string endpoint, SocketUserMessage message = null, string term = null, string location = null)
        {
            switch (endpoint)
            {
                case "wikipedia":
                    return request2wikiAsync(term).Result;
                case "cleverapi":
                    return request2cleverapiAsync(message).Result;
                case "weatherapi":
                    return request2weatherAsync(location).Result;
                case "memeapi":
                    return request2memapi().Result;
                default:
                    throw new Exception("no valid api endpoint");
            }
        }

        private static async Task<string> request2memapi()
        {
            string stringResult = await (CreateHttpRequest("https://api.memeload.us", "/v1/random").Result).Content.ReadAsStringAsync();
            return stringResult;
        }

        private static async Task<string> request2weatherAsync(string location = null)
        {
            if (location == null)
            {
                // This line gives me error | not for me
                string stringResult = await (CreateHttpRequest("https://api.openweathermap.org", $"/data/2.5/weather?q={HttpUtility.UrlEncode(Program.BotConfiguration.WeatherPlace, Encoding.UTF8)}&appid={Program.BotConfiguration.WeatherApiKey}&units=metric").Result).Content.ReadAsStringAsync();
                return stringResult;
            }
            else
            {
                string stringResult = await (CreateHttpRequest("https://api.openweathermap.org", $"/data/2.5/weather?q={HttpUtility.UrlEncode(location, Encoding.UTF8)}&appid={Program.BotConfiguration.WeatherApiKey}&units=metric").Result).Content.ReadAsStringAsync();
                return stringResult;
            }
        }

        private static async Task<string> request2cleverapiAsync(SocketUserMessage message)
        {
            string stringResult = await (CreateHttpRequest("https://www.cleverbot.com", $"/getreply?key={Program.BotConfiguration.CleverApi}&input={message.Content}").Result).Content.ReadAsStringAsync();
            return stringResult;
        }

        private static async Task<string> request2wikiAsync(string term)
        {
            string stringResult = await (CreateHttpRequest("https://en.wikipedia.org", $"/w/api.php?action=opensearch&format=json&search={term}&namespace=0&limit=5&utf8=1").Result).Content.ReadAsStringAsync();
            return stringResult;
        }

        private static async Task<HttpResponseMessage> CreateHttpRequest(string url, string urlExtension)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            HttpResponseMessage responseMessage = await client.GetAsync(urlExtension);
            return responseMessage;
        }
    }
}
