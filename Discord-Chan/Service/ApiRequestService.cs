using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sally_NET.ApiReference;
using Sally_NET.Core.Enum;
using System;
using System.Collections.Generic;
using System.IO;
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
#if RELEASE
        private const int pageResultLimit = 7;
#endif
#if DEBUG
        private const int pageResultLimit = 1;
#endif
        private static HttpClient client = new HttpClient();

        public static async Task<string> request2weatherAsync(string location = null)
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

        public static async Task<string> request2cleverapiAsync(SocketUserMessage message)
        {
            string stringResult = await (CreateHttpRequest("https://www.cleverbot.com", $"/getreply?key={Program.BotConfiguration.CleverApi}&input={message.Content}").Result).Content.ReadAsStringAsync();
            return stringResult;
        }

        public static async Task<string> request2wikiAsync(string term)
        {
            string stringResult = await (CreateHttpRequest("https://en.wikipedia.org", $"/w/api.php?action=opensearch&format=json&search={term}&namespace=0&limit=5&utf8=1").Result).Content.ReadAsStringAsync();
            return stringResult;
        }

        public static async Task<HttpResponseMessage> CreateHttpRequest(string url, string urlExtension)
        {
            client.BaseAddress = new Uri(url);
            HttpResponseMessage responseMessage = await client.GetAsync(urlExtension);
            return responseMessage;
        }

        public static async Task<string> request2konachanAsync()
        {
            string response = await (CreateHttpRequest("https://konachan.com", "/post.json?limit=100").Result).Content.ReadAsStringAsync();
            if (response == "[]")
            {
                return "";
            }
            List<KonachanApi> imageCollection = JsonConvert.DeserializeObject<List<KonachanApi>>(response);
            Random rng = new Random();
            int randImage = rng.Next(imageCollection.Count());
            return imageCollection[randImage].ImageUrl;
        }
        public static async Task<string> request2konachanAsync(string[] tags)
        {

            string tagUrl = "";
            JObject parsedResponse = new JObject();
            int pageCounter = 0;
            const int limit = 90;
            List<string> responseCollector = new List<string>();
            foreach (string tag in tags)
            {
                tagUrl = tagUrl + $"{tag}%20";
            }
            string response = String.Empty;
            //make multiple http requests, so there is more variety
            //it may occure that the randImage index is out of bound. think about, creating a new construct to store all parts of the response and work with that
            //caching response. current matrix[99][8]. there is no need for refreshing the response with every new command
            //the size of the matrix sets a good portion of randomness
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
            List<KonachanApi> imageCollection = JsonConvert.DeserializeObject<List<KonachanApi>>((responseCollector.ToArray())[randPage]);
            int randImage = rng.Next(imageCollection.Count());
            checkAndSaveTagPopularity(tagUrl);
            return imageCollection[randImage].ImageUrl;
        }

        public static async Task<string> request2konachanAsync(string[] tags, Rating rating)
        {
            int pageCounter = 0;
            const int limit = 90;
            string tagUrl = String.Empty;
            string response = String.Empty;
            List<string> responseCollector = new List<string>();
            List<KonachanApi> imageCollection = new List<KonachanApi>();
            //convert string array to string, so you can pass it in the url
            foreach (string tag in tags)
            {
                tagUrl = tagUrl + $"{tag}%20";
            }
            //create http request and get result
            while (response != "[]" && pageCounter < pageResultLimit)
            {
                response = await (CreateHttpRequest("https://konachan.com", $"/post.json?limit={limit}&tags={tagUrl}&page={pageCounter}").Result).Content.ReadAsStringAsync();
                if(response != "[]")
                {
                    imageCollection.AddRange(JsonConvert.DeserializeObject<List<KonachanApi>>(response));
                }
                pageCounter++;
            }

            if (response == "[]" && pageCounter == 1)
            {
                return "";
            }
            Random rng = new Random();
            int randImage = rng.Next(imageCollection.Count());
            //get value from enum
            Type enumType = typeof(Rating);
            MemberInfo[] memInfo = enumType.GetMember(rating.ToString());
            Object[] attributes = memInfo[0].GetCustomAttributes(typeof(RatingShortCutAttribute), false);
            char attributeValue = ((RatingShortCutAttribute)attributes[0]).ShortCut;
            //search through response
            List<KonachanApi> imageRatingResults = new List<KonachanApi>();
            imageRatingResults = imageCollection.FindAll(i => i.Rating == attributeValue);
            //check for added results
            if (imageRatingResults.Count == 0)
            {
                //list is empty
                //no results found
                return "";
            }
            else
            {
                checkAndSaveTagPopularity(tagUrl);
                //collection of images found
                //return random item from list
                return imageRatingResults[rng.Next(imageRatingResults.Count)].ImageUrl;
            }
        }

        private static void checkAndSaveTagPopularity(string tagUrl)
        {
            string formattedTagString = tagUrl.Replace("%20", " ");
            formattedTagString = formattedTagString.Remove(formattedTagString.Length - 1);
            const string tagJsonPath = "meta/tagPopularity.json";
            if (!Directory.Exists("meta"))
            {
                Directory.CreateDirectory("meta");
            }
            //check if json file exists
            if (!File.Exists(tagJsonPath))
            {
                //file doesnt exists
                File.Create(tagJsonPath);
            }
            Dictionary<string, int> tagPopularity = new Dictionary<string, int>();
            //check if json is empty
            if(JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(tagJsonPath)) != null)
            {
                tagPopularity = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(tagJsonPath));
            }
            //file exists
            //check if current search tag string already present in the json file aka dictionary
            if (tagPopularity.ContainsKey(formattedTagString))
            {
                //tag string is already a key
                //increase the counter by one
                tagPopularity[formattedTagString] += 1;
            }
            else
            {
                //tag string isnt in dictionary
                //add new entry
                tagPopularity.Add(formattedTagString, 1);
            }
            File.WriteAllText(tagJsonPath, JsonConvert.SerializeObject(tagPopularity));
        }
    }
}
