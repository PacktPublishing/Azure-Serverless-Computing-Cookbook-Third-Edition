#r "SendGrid"
#r "Newtonsoft.Json" 
using System;
using SendGrid.Helpers.Mail;
using Newtonsoft.Json;
public static void Run(string myQueueItem,out SendGridMessage message, ILogger log)
{
    log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
    dynamic inputJson = JsonConvert.DeserializeObject(myQueueItem); 
    string FirstName=null, LastName=null, Email = null; FirstName=inputJson.FirstName;
LastName=inputJson.LastName; Email=inputJson.Email;
log.LogInformation($"Email{inputJson.Email}, {inputJson.FirstName + " " + inputJson.LastName}"); 
    message = new SendGridMessage();
    message.SetSubject("New User got registered successfully."); message.SetFrom("donotreply@example.com"); message.AddTo(Email,FirstName + " " + LastName);

message.AddContent("text/html", "Thank you " + FirstName + " " + LastName +" so much for getting registered to our site.");

}
