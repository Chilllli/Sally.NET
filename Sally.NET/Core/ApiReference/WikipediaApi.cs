using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.Core.ApiReference
{
    public class WikipediaRequest
    {
        public string SearchTerm { get; set; }
        public string[] PossibleResults { get; set; }
        public string[] PossibleURLs { get; set; }
    }

    //TODO: overthink how to deserialize this json request 
    public class WikipediaApi
    {
        public WikipediaRequest[] Records { get; set; }
    }

    public class WikipediaJsonConverter : JsonConverter<WikipediaApi>
    {
        public override WikipediaApi ReadJson(JsonReader reader, Type objectType, WikipediaApi existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JArray jArray = JArray.Load(reader);
            WikipediaApi result = new();
            result.Records = new WikipediaRequest[]
            {
                new WikipediaRequest()
                {
                    SearchTerm = jArray[0].ToString(),
                    PossibleResults = jArray[1].ToObject<string[]>(),
                    PossibleURLs = jArray[3].ToObject<string[]>()
                }
            };
            return result;
        }

        public override void WriteJson(JsonWriter writer, WikipediaApi value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
