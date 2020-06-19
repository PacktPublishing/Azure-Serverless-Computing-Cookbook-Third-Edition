using System;
using System.Configuration;
using System.IO;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;

namespace CreateQueueMessage
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateQueueMessages();
            Console.WriteLine("Hello World!");
        }
        static void CreateQueueMessages()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("StorageConnectionString"));
            CloudQueueClient queueclient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueclient.GetQueueReference("myqueuemessages");
            queue.CreateIfNotExists();

            CloudQueueMessage message = null;
            for (int nQueueMessageIndex = 0; nQueueMessageIndex <= 100; nQueueMessageIndex++)
            {

                message = new CloudQueueMessage(Convert.ToString(nQueueMessageIndex));
                queue.AddMessage(message); Console.WriteLine(nQueueMessageIndex);
            }
        }

    }
}
