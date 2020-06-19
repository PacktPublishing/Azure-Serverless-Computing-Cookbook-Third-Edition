using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Threading.Tasks;


namespace CSVImport.DurableFunctions
{
    class StorageManager
    {
        public async static Task<string> ReadBlob(string BlobName)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(configuration["Values:StorageConnection"]);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient(); CloudBlobContainer CSVBlobContainer = cloudBlobClient.GetContainerReference("csvimports");
            CloudBlockBlob cloudBlockBlob = CSVBlobContainer.GetBlockBlobReference(BlobName);
            string employeeContent;
            using (var memoryStream = new MemoryStream())
            {
                await cloudBlockBlob.DownloadToStreamAsync(memoryStream);
                employeeContent = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            return employeeContent;
        }
    }

}
