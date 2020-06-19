using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
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
            }
            return outputs;
        }

        [FunctionName("ReadCSV_AT")]
        public static async Task<List<Employee>> ReadCSV_AT([ActivityTrigger] string name,
 ILogger log)
        {
            log.LogInformation("ReadExcel_AT Started");
            log.LogInformation("Reading the Blob Started");
            var EmployeeContent = await StorageManager.ReadBlob(name);
            log.LogInformation("Reading the Blob has Completed");
            log.LogInformation("Reading the Excel Data Started");
            List<Employee> employees = CSVManager.ReadEmployeeData(EmployeeContent);
            log.LogInformation("Reading the Blob has Completed");

            log.LogInformation("ReadExcel_AT End");
            return employees;
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