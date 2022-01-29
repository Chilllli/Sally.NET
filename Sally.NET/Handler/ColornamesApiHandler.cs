using Newtonsoft.Json;
using Sally.NET.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.Handler
{
    public class ColornamesApiHandler : HttpRequestBase
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly Uri uri = new Uri("https://colornames.org");
        public ColornamesApiHandler()
        {
            httpClient.BaseAddress = uri;
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
        private async Task<string> Request2ColorNamesApiAsync(string hexcode)
        {
            hexcode = hexcode.ToUpper();
            string response = await (CreateHttpRequest(httpClient, $"/search/json/?hex={hexcode}").Result).Content.ReadAsStringAsync();
            dynamic jsonData = JsonConvert.DeserializeObject<dynamic>(response);
            if (jsonData["name"] == null)
            {
                return null;
            }
            else
            {
                return jsonData["name"];
            }
        }

        public string GetColorName(string color)
        {
            return Request2ColorNamesApiAsync(color).Result;
        }
    }
}
