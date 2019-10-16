using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static Sally_NET.Command.PictureCommands;

namespace Sally_NET.Service
{
    static class ApiRequestService
    {
        public static async Task<string> StartRequest(string endpoint, SocketUserMessage message = null, string term = null, string location = null, string[] tags = null, Rating? rating = null)
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
                case "konachan":
                    return request2konachan().Result;
                case "konachanWithTag":
                    return request2konachan(tags).Result;
                case "konachanWithRating":
                    return request2konachan(tags, rating.Value).Result;
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

        private static async Task<string> request2konachan()
        {
            string response = await (CreateHttpRequest("https://konachan.com", "/post.json?limit=100").Result).Content.ReadAsStringAsync();
            if(response == "[]")
            {
                return "";
            }
            dynamic dynResponse = JsonConvert.DeserializeObject<dynamic>(response);
            Random rng = new Random();
            int randImage = rng.Next(Enumerable.Count(dynResponse));
            return dynResponse[randImage]["jpeg_url"];
        }
        private static async Task<string> request2konachan(string[] tags)
        {
            JObject parsedResponse = new JObject();
            int pageCounter = 0;
            string tagUrl = "";
            const int limit = 50;
            const int pageResultLimit = 2;
            List<string> responseCollector = new List<string>();

            foreach (string tag in tags)
            {
                tagUrl = tagUrl + $"{tag}%20";
            }

            string response = String.Empty;

            //make multiple http requests, so there is more variety
            while (response != "[]" && pageCounter < pageResultLimit)
            {
                response = await (CreateHttpRequest("https://konachan.com", $"/post.json?limit={limit}&tags={tagUrl}&page={pageCounter}").Result).Content.ReadAsStringAsync();
                responseCollector.Add(response);
                pageCounter++;
            }

            if (response == "[]" && pageCounter == 1)
            {
                return "";
            }
            Random rng = new Random();
            //int randImage = rng.Next(limit);
            int randPage = rng.Next(pageCounter);
            dynamic dynResponse = JsonConvert.DeserializeObject<dynamic>((responseCollector.ToArray())[randPage]);
            int randImage = rng.Next(Enumerable.Count(dynResponse));
            return (string)dynResponse[randImage]["jpeg_url"];

        }

        private static async Task<string> request2konachan(string[] tags, Rating rating)
        {
            const int limit = 50; 
            string tagUrl = String.Empty;
            string response = String.Empty;
            //convert string array to string, so you can pass it in the url
            foreach (string tag in tags)
            {
                tagUrl = tagUrl + $"{tag}%20";
            }
            //create http request and get result
            response = await (CreateHttpRequest("https://konachan.com", $"/post.json?limit={limit}&tags={tagUrl}").Result).Content.ReadAsStringAsync();
            //check if response is empty
            if(response == "[]")
            {
                return "";
            }
            Random rng = new Random();
            int randImage = rng.Next(limit);
            dynamic dynResponse = JsonConvert.DeserializeObject<dynamic>(response);
            //get value from enum
            Type enumType = typeof(Rating);
            MemberInfo[] memInfo = enumType.GetMember(rating.ToString());
            Object[] attributes = memInfo[0].GetCustomAttributes(typeof(RatingShortCutAttribute), false);
            string attributeValue = ((RatingShortCutAttribute)attributes[0]).ShortCut;
            //search through response
            List<string> imageRatingResults = new List<string>();
            for (int i = 0; i < limit; i++)
            {
                //check item for rating
                if(dynResponse[i]["rating"] == attributeValue)
                {
                    //image found with rating
                    imageRatingResults.Add((string)dynResponse[i]["jpeg_url"]);
                }
            }
            //check for added results
            if(imageRatingResults.Count == 0)
            {
                //list is empty
                //no results found
                return "";
            }
            else
            {
                //collection of images found
                //return random item from list
                return imageRatingResults[rng.Next(imageRatingResults.Count)];
            }
        }
    }
}
