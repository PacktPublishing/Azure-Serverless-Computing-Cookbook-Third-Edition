using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CSVImport.DurableFunctions
{
    public static class CSVImportBlobTrigger
    {
        [FunctionName("CSVImportBlobTrigger")]
        public static void Run([BlobTrigger("csvimports/{name}", Connection = "StorageConnection")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
