using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.Core
{
    public abstract class HttpRequestBase
    {
        public async Task<HttpResponseMessage> CreateHttpRequest(HttpClient httpClient, string urlExtension)
        {
            return await httpClient.GetAsync(urlExtension);
        }
    }
}
