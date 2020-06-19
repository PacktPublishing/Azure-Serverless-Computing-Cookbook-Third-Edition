#r "SendGrid"
#r "Newtonsoft.Json" 
#r "Microsoft.Azure.WebJobs.Extensions.Storage"

using System;
using SendGrid.Helpers.Mail; 
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Storage;

public static void Run(string myQueueItem,out SendGridMessage message, IBinder binder, ILogger log)

{
     log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
     
     dynamic inputJson = JsonConvert.DeserializeObject(myQueueItem);
     string FirstName=null, LastName=null, Email = null; 
     string emailContent;
    
     FirstName=inputJson.FirstName; 
     LastName=inputJson.LastName; 
     Email=inputJson.Email;
    
     log.LogInformation($"Email{inputJson.Email}, {inputJson.FirstName + " " + inputJson.LastName}"); message = new SendGridMessage();
     
     message.SetSubject("New User got registered successfully."); 
     message.SetFrom("donotreply@example.com");
     message.AddTo(Email,FirstName + " " + LastName);

     emailContent = "Thank you <b>" + FirstName + " " + LastName +"</b> for your registration.<br><br>" + "Below are the details that you have provided us<br> <br>"+ "<b>First name:</b> " + FirstName + "<br>" + "<b>Last name:</b> " + LastName + "<br>" + "<b>Email Address:</b> " + inputJson.Email + "<br><br> <br>" + "Best Regards," + "<br>" + "Website Team";


    message.AddContent("text/html", emailContent);

    log.LogInformation($"Row key: {inputJson.RowKey}");
    
    using (var emailLogBloboutput = binder.Bind<TextWriter>(new BlobAttribute($"userregistrationemaillogs/{inputJson.RowKey}.log")))
    {
        emailLogBloboutput.WriteLine(emailContent);
    }

   message.AddAttachment(FirstName +"_"+LastName+".log",
                              System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(emailContent)),
                             "text/plain",
                             "attachment",
                              "Logs"
                               );
}
