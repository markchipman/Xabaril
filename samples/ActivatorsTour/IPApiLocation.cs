using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xabaril.Core;

namespace ActivatorsTour
{
    public class IPApiLocationProvider
        : IGeoLocationProvider
    {
        static HttpClient _client = new HttpClient() { BaseAddress = new Uri("http://ip-api.com/json") };

        public async Task<string> FindLocationAsync(string ipAddress)
        {
            var response = await _client.GetAsync(ipAddress);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                dynamic result = JObject.Parse(content);

                return result.country;
            }

            return null;
        }
    }
}
