﻿using Sally.NET.Core;
using Sally.NET.Core.ApiReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.Handler
{
    public class WikipediaApiHandler : HttpRequestBase
    {
        private readonly HttpClient httpClient;

        public WikipediaApiHandler(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// The <c>Request2WikipediaApiAsync</c> method creates a api call to the wikipedia api.
        /// </summary>
        /// <param name="term">A term, which is looked up in wikipedia.</param>
        /// <returns>Returns a json string result from the api call.</returns>
        public async Task<string> Request2WikipediaApiAsync(string term)
        {
            return await (await CreateHttpRequest(httpClient, $"/w/api.php?action=opensearch&format=json&search={term}&namespace=0&limit=5&utf8=1")).Content.ReadAsStringAsync();
        }
    }
}
