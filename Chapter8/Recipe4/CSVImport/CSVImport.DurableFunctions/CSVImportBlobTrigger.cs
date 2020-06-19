using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;


namespace CSVImport.DurableFunctions
{
    public static class CSVImportBlobTrigger
    {
        [FunctionName("CSVImportBlobTrigger")]
        public static async  void Run([BlobTrigger("csvimports/{name}", Connection = "StorageConnection")]Stream myBlob, string name, [DurableClient]IDurableOrchestrationClient starter, ILogger log)
        {
            string instanceId = await starter.StartNewAsync("CSVImport_Orchestrator", name); log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
