using System.Text;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace WebApplication.Models
{
    public static class VectorRepository
    {
        private static readonly HttpClient client = new HttpClient();

        private static string version = "?api-version=2023-02-01-preview&modelVersion=latest";

        public static async Task<List<ResultSearch>> DoVectorSearch(string searchPrompt, IConfiguration conf)
        {
            var textVector = await TextEmbedding(searchPrompt, conf);

            var payload = new
            {
                vectorQueries = new[]
                {
                    new
                    {
                        kind = "vector",
                        vector = textVector,
                        fields = "contentVector",
                        k = 10
                    }
                },
                select = "id, url",
                count = true
            };

            HttpRequestMessage message = new HttpRequestMessage()
            {
                RequestUri = new Uri(conf["AzureAiSearchEndpoint"]),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };

            message.Headers.Add("api-key", conf["AzureAiSearchKey"]);

            var response = await client.SendAsync(message);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var o = JObject.Parse(responseJson);

            var res = new List<ResultSearch>();

            foreach (var result in o.GetValue("value").AsEnumerable())
            {
                res.Add(
                    new ResultSearch() { 
                        id = ((JObject)result).GetValue("id").ToString(), 
                        url = ((JObject)result).GetValue("url").ToString() 
                    }
                    );                
            }            

            return res;
        }

        private static async Task<float[]> TextEmbedding(string text, IConfiguration conf)
        {
            string vecTxtUrl = conf["AzureComputerVisionEndpoint"] + "/computervision/retrieval:vectorizeText" + version;

            HttpRequestMessage message = new HttpRequestMessage()
            {
                RequestUri = new Uri(vecTxtUrl),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(new { text = text }), Encoding.UTF8, "application/json")
            };
            message.Headers.Add("Ocp-Apim-Subscription-Key", conf["AzureComputerVisionKey"]);

            var response = await client.SendAsync(message);
            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObj = JsonConvert.DeserializeObject<dynamic>(responseJson);
            var vector = responseObj.vector.ToObject<float[]>();

            return vector;
        }
    }
}
