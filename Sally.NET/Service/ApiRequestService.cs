using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sally.NET.Core.ApiReference;
using Sally.NET.Core.Configuration;
using Sally.NET.Core.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sally.NET.Service
{
    /// <summary>
    /// The <c>ApiRequestService</c> class handles all api requests to other web apis.
    /// </summary>
    public static class ApiRequestService
    {
#if RELEASE
        private const int pageResultLimit = 7;
#endif
#if DEBUG
        private const int pageResultLimit = 1;
#endif
        private static BotCredentials credentials;

        /// <summary>
        /// The <c>Initialize</c> method initialize this service with specific bot credentials.
        /// </summary>
        /// <param name="credentials">
        /// Botcredentials with set values
        /// </param>
        /// <returns></returns>
        public static void Initialize(BotCredentials credentials)
        {
            ApiRequestService.credentials = credentials;
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
        public static async Task<string> Request2WeatherApiAsync(string location = null)
        {
            if (location == null)
            {
                // This line gives me error | not for me
                string stringResult = await (CreateHttpRequest("https://api.openweathermap.org", $"/data/2.5/weather?q={HttpUtility.UrlEncode(credentials.WeatherPlace, Encoding.UTF8)}&appid={credentials.WeatherApiKey}&units=metric").Result).Content.ReadAsStringAsync();
                return stringResult;
            }
            else
            {
                string stringResult = await (CreateHttpRequest("https://api.openweathermap.org", $"/data/2.5/weather?q={HttpUtility.UrlEncode(location, Encoding.UTF8)}&appid={credentials.WeatherApiKey}&units=metric").Result).Content.ReadAsStringAsync();
                return stringResult;
            }
        }

        /// <summary>
        /// The <c>Request2CleverBotApiASync</c> method creates an api call to the cleverbot api.
        /// </summary>
        /// <param name="message">Direct message from a user</param>
        /// <returns>Returns json data strong from the api call</returns>
        /// <remarks><b>If the cleverbot api key is not set in the config file, then this method won't work.</b></remarks>
        public static async Task<string> Request2CleverBotApiAsync(SocketUserMessage message)
        {
            string stringResult = await (CreateHttpRequest("https://www.cleverbot.com", $"/getreply?key={credentials.CleverApi}&input={message.Content}").Result).Content.ReadAsStringAsync();
            return stringResult;
        }

        /// <summary>
        /// The <c>Request2WikipediaApiAsync</c> method creates a api call to the wikipedia api.
        /// </summary>
        /// <param name="term">A term, which is looked up in wikipedia.</param>
        /// <returns>Returns a json string result from the api call.</returns>
        public static async Task<string> Request2WikipediaApiAsync(string term)
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

        /// <summary>
        /// The <c>Request2KonachanApiAsync</c> method creates a generic konachan web api request.
        /// </summary>
        /// <remarks>
        /// <b>Tags</b> and <paramref name="Rating"/> are ignored. <br />
        /// See <see cref="Request2KonachanApiAsync(string[])"/> to submit image tags to the image search. <br />
        /// See <see cref="Request2KonachanApiAsync(string[], Rating)"/> to add tags with a specific rating to the image search.
        /// </remarks>
        /// <returns>Returns string image url of a random selected image.</returns>
        /// <example>
        /// <code>
        /// ApiRequestService.Request2KonachanApiAsync()
        /// 
        /// Result:
        /// 
        /// {
        ///     "id":317653,
        ///     "tags":"aoi_tori aqua_eyes brown_hair chaamii maid umino_akari",
        ///     "created_at":1603482940,
        ///     "creator_id":140316,
        ///     ...
        /// }
        /// </code>
        /// </example>
        public static async Task<string> Request2KonachanApiAsync()
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

        /// <summary>
        /// The <c>Request2KonachanApiAsync</c> method creates a konachan api call with certain tags. <br />
        /// <paramref name="Rating"/> is ignored.
        /// </summary>
        /// <param name="tags">The parameter is a array of strings (tags) for a filtered immage search.</param>
        /// <returns>Returns a filtered json string result from the api.</returns>
        /// <example>
        /// <see cref="Request2KonachanApiAsync()"/>
        /// </example>
        public static async Task<string> Request2KonachanApiAsync(string[] tags)
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
            string formattedTagString = tagUrl.Replace("%20", " ");
            formattedTagString = formattedTagString.Remove(formattedTagString.Length - 1);
            List<KonachanApi> imageCollection = new List<KonachanApi>();
            if (File.Exists($"cached/{formattedTagString}.json"))
            {

                // read JSON directly from a file
                using (StreamReader file = File.OpenText($"cached/{formattedTagString}.json"))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JToken o2 = JToken.ReadFrom(reader);
                    imageCollection = JsonConvert.DeserializeObject<List<KonachanApi>>(o2.ToString());
                }
            }
            else
            {
                while (response != "[]" && pageCounter < pageResultLimit)
                {
                    response = await (await CreateHttpRequest("https://konachan.com", $"/post.json?limit={limit}&tags={tagUrl}&page={pageCounter}")).Content.ReadAsStringAsync();

                    if (response != "[]")
                    {
                        imageCollection.AddRange(JsonConvert.DeserializeObject<List<KonachanApi>>(response));
                    }
                    responseCollector.Add(response);
                    pageCounter++;
                }

                if (response == "[]" && pageCounter == 1)
                {
                    return "";
                }
                saveJsonToFile(imageCollection, formattedTagString);
            }
            Random rng = new Random();
            //int randImage = rng.Next(limit);
            int randPage = rng.Next(pageCounter);
            int randImage = rng.Next(imageCollection.Count());
            checkAndSaveTagPopularity(tagUrl);
            return imageCollection[randImage].ImageUrl;
        }

        /// <summary>
        /// The <c>Request2KonachanApiAsync</c> method creates a konachan api call with certain tags and a specific rating.
        /// </summary>
        /// <param name="tags">The parameter is a array of strings (tags) for a filtered immage search.</param>
        /// <param name="rating">The parameter meter is an enum of aspecific rating. </param>
        /// <returns>Returns a filtered json string result from the api.</returns>
        /// <example>
        /// <see cref="Request2KonachanApiAsync()"/>
        /// </example>
        public static async Task<string> Request2KonachanApiAsync(string[] tags, Rating rating)
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
            string formattedTagString = tagUrl.Replace("%20", " ");
            if (!String.IsNullOrEmpty(formattedTagString))
                formattedTagString = formattedTagString.Remove(formattedTagString.Length - 1);
            //create http request and get result
            if (File.Exists($"cached/{formattedTagString}.json"))
            {


                // read JSON directly from a file
                using (StreamReader file = File.OpenText($"cached/{formattedTagString}.json"))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JToken o2 = JToken.ReadFrom(reader);
                    imageCollection = JsonConvert.DeserializeObject<List<KonachanApi>>(o2.ToString());
                }
            }
            else
            {
                while (response != "[]" && pageCounter < pageResultLimit)
                {
                    response = await (await CreateHttpRequest("https://konachan.com", $"/post.json?limit={limit}&tags={tagUrl}&page={pageCounter}")).Content.ReadAsStringAsync();

                    if (response != "[]")
                    {
                        imageCollection.AddRange(JsonConvert.DeserializeObject<List<KonachanApi>>(response));
                    }
                    responseCollector.Add(response);
                    pageCounter++;
                }

                if (response == "[]" && pageCounter == 1)
                {
                    return "";
                }
                saveJsonToFile(imageCollection, formattedTagString);
            }

            Random rng = new Random();
            int randImage = rng.Next(imageCollection.Count());
            //get value from enum
            Type enumType = typeof(Rating);
            MemberInfo[] memInfo = enumType.GetMember(rating.ToString());
            Object[] attributes = memInfo[0].GetCustomAttributes(typeof(RatingShortCutAttribute), false);
            char attributeValue = ((RatingShortCutAttribute)attributes[0]).ShortCut;
            //search through response
            List<KonachanApi> imageRatingResults = imageCollection.FindAll(i => i.Rating == attributeValue);
            //check for added results
            if (imageRatingResults.Count == 0)
            {
                //list is empty
                //no results found
                return "";
            }
            else
            {
                if(!String.IsNullOrEmpty(formattedTagString))
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
            //check if json file exists
            if (!File.Exists(tagJsonPath))
            {
                //file doesnt exists
                File.Create(tagJsonPath);
            }
            Dictionary<string, int> tagPopularity = new Dictionary<string, int>();
            //check if json is empty
            if (JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(tagJsonPath)) != null)
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

        private static void saveJsonToFile(List<KonachanApi> json, string tagString)
        {
            string cachePath = $"cached/{tagString}.json";
            //check if json file exists
            if (!File.Exists(cachePath))
            {
                //file doesnt exists
                File.WriteAllText(cachePath, JsonConvert.SerializeObject(json));
            }
        }

        /// <summary>
        /// The <c>Request2ColorNamesApiAsync</c> method creates a api call to the color names api.
        /// </summary>
        /// <param name="hexcode">The parameter is a hexcode string.</param>
        /// <returns>Returns a json string result of color name and hexcode.</returns>
        /// <example>
        /// <code>
        /// ApiReuqestService.Request2ColorNamesApiAsync("FFFFFF")
        /// 
        /// Result:
        /// 
        /// {
        ///     "hexCode":"ffffff",
        ///     "name":"White"
        /// }
        /// </code>
        /// </example>
        public static async Task<string> Request2ColorNamesApiAsync(string hexcode)
        {
            hexcode = hexcode.ToUpper();
            string response = await (CreateHttpRequest("https://colornames.org", $"/search/json/?hex={hexcode}").Result).Content.ReadAsStringAsync();
            dynamic jsonData = JsonConvert.DeserializeObject<dynamic>(response);
            //check if name value exists
            if (jsonData["name"] == null)
            {
                return null;
            }
            else
            {
                return jsonData["name"];
            }
        }
    }
}
