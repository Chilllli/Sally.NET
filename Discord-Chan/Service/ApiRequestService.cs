using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Discord_Chan.Service
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
                default:
                    throw new Exception("no valid api endpoint");
            }
        }

        private static async Task<string> request2weatherAsync(string location = null)
        {
            if (location == null)
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://api.openweathermap.org");
                HttpResponseMessage response = await client.GetAsync($"/data/2.5/weather?q={HttpUtility.UrlEncode(Program.BotConfiguration.WeatherPlace, Encoding.UTF8)}&appid={Program.BotConfiguration.WeatherApiKey}&units=metric");

                // This line gives me error | not for me
                string stringResult = await response.Content.ReadAsStringAsync();
                return stringResult;
            }
            else
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://api.openweathermap.org");
                HttpResponseMessage response = await client.GetAsync($"/data/2.5/weather?q={HttpUtility.UrlEncode(location, Encoding.UTF8)}&appid={Program.BotConfiguration.WeatherApiKey}&units=metric");

                string stringResult = await response.Content.ReadAsStringAsync();
                return stringResult;
            }
        }

        private static async Task<string> request2cleverapiAsync(SocketUserMessage message)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://www.cleverbot.com");
            HttpResponseMessage response = await client.GetAsync($"/getreply?key={Program.BotConfiguration.CleverApi}&input={message.Content}");

            string stringResult = await response.Content.ReadAsStringAsync();
            return stringResult;
        }

        private static async Task<string> request2wikiAsync(string term)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://en.wikipedia.org");
            HttpResponseMessage response = await client.GetAsync($"/w/api.php?action=opensearch&format=json&search={term}&namespace=0&limit=5&utf8=1");

            string stringResult = await response.Content.ReadAsStringAsync();
            return stringResult;
        }
    }
}
