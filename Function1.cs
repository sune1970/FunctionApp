using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Homework.Function
{
    public class ApiCallLog
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Timetamp { get; set; }
        public string Description { get; set; }

        public string Body { get; set; }
    }

    public class Function1
    {
        [FunctionName("Function1")]
        public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer,
            [Table("ApiCallLog"), StorageAccount("AzureWebJobsStorage")] ICollector<ApiCallLog> table,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            await CallRandomApi(table);
        }
        public static async Task CallRandomApi(ICollector<ApiCallLog> table)
        {
            string URL = " https://api.publicapis.org";
            string urlParameters = "/random?auth=null";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.GetAsync(urlParameters).Result;

            if (response.IsSuccessStatusCode)
            {
                LogInfo(response, table);
            }
            else
            {
                LogError(response, table);
            }

            client.Dispose();
        }

        public static void LogError(HttpResponseMessage response, ICollector<ApiCallLog> table)
        {
            ApiCallLog log = new ApiCallLog
            {
                PartitionKey = "Error",
                RowKey = Guid.NewGuid().ToString(),
                Timetamp = DateTime.Now,
                Description = $"{(int)response.StatusCode}, {response.ReasonPhrase}"
            };

            table.Add(log);
        }

        public static void LogInfo(HttpResponseMessage response, ICollector<ApiCallLog> table)
        {
            ApiCallLog log = new ApiCallLog
            {
                PartitionKey = "Info",
                RowKey = Guid.NewGuid().ToString(),
                Timetamp = DateTime.Now,
                Description = $"{(int)response.StatusCode}, {response.ReasonPhrase}",
            };

            table.Add(log);
        }
    }

}
