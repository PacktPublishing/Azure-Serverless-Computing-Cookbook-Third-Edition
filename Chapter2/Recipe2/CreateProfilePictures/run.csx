/*using System;

public static void Run(string myQueueItem, ILogger log)
{
    log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
}*/
using System;
public static void Run(Stream outputBlob,string myQueueItem, ILogger log)
{
byte[] imageData = null;
using (var wc = new System.Net.WebClient())
{
imageData = wc.DownloadData(myQueueItem);
}
outputBlob.WriteAsync(imageData,0,imageData.Length);
}
