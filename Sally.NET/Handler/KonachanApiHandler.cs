using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sally.NET.Core;
using Sally.NET.Core.ApiReference;
using Sally.NET.Core.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.Handler
{
    public class KonachanApiHandler : HttpRequestBase
    {

#if RELEASE
        private const int pageResultLimit = 7;
#endif
#if DEBUG
        private const int pageResultLimit = 1;
#endif
        private readonly Uri konachanUri = new Uri("https://konachan.com");
        private readonly HttpClient httpClient = new HttpClient();

        public KonachanApiHandler()
        {
            httpClient.BaseAddress = konachanUri;
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
        private async Task<string> Request2KonachanApiAsync()
        {
            string response = await CreateHttpRequest(httpClient, "/post.json?limit=100").Result.Content.ReadAsStringAsync();
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
        private async Task<string> Request2KonachanApiAsync(string[] tags)
        {
            int pageCounter = 0;
            const int limit = 90;
            List<string> responseCollector = new List<string>();
            StringBuilder tagUrl = new StringBuilder();
            foreach (string tag in tags)
            {
                tagUrl.Append($"{tag}%20");
            }
            string response = string.Empty;
            //make multiple http requests, so there is more variety
            //it may occure that the randImage index is out of bound. think about, creating a new construct to store all parts of the response and work with that
            //caching response. current matrix[99][8]. there is no need for refreshing the response with every new command
            //the size of the matrix sets a good portion of randomness
            string formattedTagString = tagUrl.ToString().Replace("%20", " ");
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
                    response = await (await CreateHttpRequest(httpClient, $"/post.json?limit={limit}&tags={tagUrl}&page={pageCounter}")).Content.ReadAsStringAsync();

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
            int randImage = rng.Next(imageCollection.Count());
            checkAndSaveTagPopularity(tagUrl.ToString());
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
        private async Task<string> Request2KonachanApiAsync(string[] tags, Rating rating)
        {
            int pageCounter = 0;
            const int limit = 90;
            string tagUrl = string.Empty;
            string response = string.Empty;
            List<string> responseCollector = new List<string>();
            List<KonachanApi> imageCollection = new List<KonachanApi>();
            //convert string array to string, so you can pass it in the url
            foreach (string tag in tags)
            {
                tagUrl = tagUrl + $"{tag}%20";
            }
            string formattedTagString = tagUrl.Replace("%20", " ");
            if (!string.IsNullOrEmpty(formattedTagString))
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
                    response = await (await CreateHttpRequest(httpClient, $"/post.json?limit={limit}&tags={tagUrl}&page={pageCounter}")).Content.ReadAsStringAsync();

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
            object[] attributes = memInfo[0].GetCustomAttributes(typeof(RatingShortCutAttribute), false);
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
                if (!string.IsNullOrEmpty(formattedTagString))
                    checkAndSaveTagPopularity(tagUrl);
                //collection of images found
                //return random item from list
                return imageRatingResults[rng.Next(imageRatingResults.Count)].ImageUrl;
            }
        }

        private void checkAndSaveTagPopularity(string tagUrl)
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

        private void saveJsonToFile(List<KonachanApi> json, string tagString)
        {
            string cachePath = $"cached/{tagString}.json";
            //check if json file exists
            if (!File.Exists(cachePath))
            {
                //file doesnt exists
                File.WriteAllText(cachePath, JsonConvert.SerializeObject(json));
            }
        }

        public async Task<string> GetKonachanPictureUrl()
        {
            return await Request2KonachanApiAsync();
        }

        public string GetKonachanPictureUrl(string[] tagCollection)
        {
            return Request2KonachanApiAsync(tagCollection).Result;
        }

        public string GetKonachanPictureUrl(string[] tagCollection, Rating rating)
        {
            return Request2KonachanApiAsync(tagCollection, rating).Result;
        }
    }
}
