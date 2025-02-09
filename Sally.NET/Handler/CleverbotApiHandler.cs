using Discord.WebSocket;
using Sally.NET.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.Handler
{
    public class CleverbotApiHandler : HttpRequestBase
    {
        private readonly HttpClient httpClient;

        public CleverbotApiHandler(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// The <c>Request2CleverBotApiASync</c> method creates an api call to the cleverbot api.
        /// </summary>
        /// <param name="message">Direct message from a user</param>
        /// <returns>Returns json data strong from the api call</returns>
        /// <remarks><b>If the cleverbot api key is not set in the config file, then this method won't work.</b></remarks>
        public async Task<string> Request2CleverBotApiAsync(SocketUserMessage message, string apiKey)
        {
            return await (CreateHttpRequest(httpClient, $"/getreply?key={apiKey}&input={message.Content}").Result).Content.ReadAsStringAsync();
        }
    }
}
