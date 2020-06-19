using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CSVImport.DurableFunctions
{
    public static class CSVImport_Orchestrator
    {
        [FunctionName("CSVImport_Orchestrator")]
        public static async Task<List<string>> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();
            string CSVFileName = context.GetInput<string>();
            {
                List<Employee> employees = await context.CallActivityAsync<List<Employee>>("ReadCSV_AT", CSVFileName);
                await context.CallActivityAsync<string>("ScaleRU_AT", 500);
                await context.CallActivityAsync<string>("ImportData_AT", employees);
            }
            return outputs;
        }

        [FunctionName("ReadCSV_AT")]
        public static async Task<List<Employee>> ReadCSV_AT([ActivityTrigger] string name,
 ILogger log)
        {
            log.LogInformation("ReadCSV_AT Started");
            log.LogInformation("Reading the Blob Started");
            var EmployeeContent = await StorageManager.ReadBlob(name);
            log.LogInformation("Reading the Blob has Completed");
            log.LogInformation("Reading the CSV Data Started");
            List<Employee> employees = CSVManager.ReadEmployeeData(EmployeeContent);
            log.LogInformation("Reading the Blob has Completed");

            log.LogInformation("ReadCSV_AT End");
            return employees;
        }

        [FunctionName("ScaleRU_AT")]
        public static async Task<string> ScaleRU_AT([ActivityTrigger] int RequestUnits, 
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnectionString")]DocumentClient client
)
        {
            DocumentCollection EmployeeCollection = await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri("cookbookdb", "EmployeeContainer"));
            Offer offer = client.CreateOfferQuery().Where(o => o.ResourceLink == EmployeeCollection.SelfLink).AsEnumerable().Single();
            Offer replaced = await client.ReplaceOfferAsync(new OfferV2(offer, RequestUnits));
            return $"The RUs are scaled to 500 RUs!";
        }
        [FunctionName("ImportData_AT")]
        public static async Task<string> ImportData_AT([ActivityTrigger] List<Employee> employees, [CosmosDB(ConnectionStringSetting =
"CosmosDBConnectionString")]DocumentClient client, ILogger log)
        {
            foreach (Employee employee in employees)
            {
                await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri("cookbookdb", "EmployeeContainer"), employee);
                log.LogInformation($"Successfully inserted {employee.Name}.");
            }
            return $"Data has been imported to Cosmos DB Container Successfully!";
        }



        [FunctionName("CSVImport_Orchestrator_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("CSVImport_Orchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("CSVImport_Orchestrator", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}