using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CSVImport.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                UploadBlob().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An Error has occurred with the message" + ex.Message);
            }
            Console.WriteLine("Successfully uploaded.");
        }

        private static async Task UploadBlob()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build(); 
            
            CloudStorageAccount cloudStorageAccount =

            CloudStorageAccount.Parse(configuration.GetConnectionString("StorageConnection"));
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient(); CloudBlobContainer CSVBlobContainer = cloudBlobClient.GetContainerReference("csvimports");
            await CSVBlobContainer.CreateIfNotExistsAsync(); CloudBlockBlob cloudBlockBlob = CSVBlobContainer.GetBlockBlobReference("employeeinformation" + Guid.NewGuid().ToString());
            await cloudBlockBlob.UploadFromFileAsync(@"C:\Cookbook\Version3\Azure Serverless computing cookbook\Chapter8\Employees.csv");
        }


    }
}
