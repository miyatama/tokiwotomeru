using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TokiwotomeruApp
{
    class LuisRequest
    {

        public async Task<LuisResult> MakeRequest(string query)
        {
            var client = new HttpClient();
            var appId = "";
            var subscriptionId = "";
            var uri = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/" + appId + "?subscription-key=" + subscriptionId + "&verbose=false&timezoneOffset=0&q=" + query;
            var response = await client.GetAsync(uri);

            var strResponseContent = await response.Content.ReadAsStringAsync();

            LuisResult result = JsonConvert.DeserializeObject<LuisResult>(strResponseContent.ToString());
            return result;
        }
    }
}
