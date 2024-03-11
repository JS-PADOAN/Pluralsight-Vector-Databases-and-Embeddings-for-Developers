using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using System.Xml.Linq;

namespace ConsoleAppIndexer
{
    internal class Program
    {
        private static readonly HttpClient client = new HttpClient();        
        private static string version = "?api-version=2023-02-01-preview&modelVersion=latest";

        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            await IndexBlobContainerImages(configuration);
        }

        private static async Task IndexBlobContainerImages(IConfiguration conf)
        {
            HttpRequestMessage message = new HttpRequestMessage()
            {
                RequestUri = new Uri(conf["AzureBlobContainerUrl"] + "?restype=container&comp=list"),
                Method = HttpMethod.Get,
            };

            var response = await client.SendAsync(message);
            var responseXML = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var xmlDoc = XDocument.Parse(responseXML);

            var blobs = from blob in xmlDoc.Descendants("Blob")
                        select new
                        {
                            Name = blob.Element("Name").Value,
                            Url = blob.Element("Url").Value
                        };

            int i = 0;
            foreach (var blob in blobs)
            {
                i++;
                await AddImageToIndex(blob.Name.Replace(".jpg", ""), blob.Url, conf);
                await Task.Delay(3000);
                Console.WriteLine($"Indexed image number {i}");
            }
        }

        private static async Task AddImageToIndex(string name, string imageUrl, IConfiguration conf)
        {
            float[] imageVector = await ImageEmbedding(imageUrl, conf);

            var payload = new
            {
                value = new[]
                {
                    new { id = name, url = imageUrl, contentVector = imageVector }
                }
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
        }

        private static async Task<float[]> ImageEmbedding(string imageUrl, IConfiguration conf)
        {
            string vecImgUrl = conf["AzureComputerVisionEndpoint"] + "/computervision/retrieval:vectorizeImage" + version;

            HttpRequestMessage message = new HttpRequestMessage()
            {
                RequestUri = new Uri(vecImgUrl),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(new { url = imageUrl }), Encoding.UTF8, "application/json")
            };

            message.Headers.Add("Ocp-Apim-Subscription-Key", conf["AzureComputerVisionKey"]);

            var response = await client.SendAsync(message);
            var responseJson = await response.Content.ReadAsStringAsync();
            dynamic responseObj = JsonConvert.DeserializeObject<dynamic>(responseJson);
            var vector = responseObj.vector.ToObject<float[]>();

            return vector;
        }
    }
}