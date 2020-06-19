#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");
    int followersCount=0;
    bool blnReturnValue=false;

    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    dynamic data = JsonConvert.DeserializeObject(requestBody);
    followersCount = data?.followersCount;
    string name = data?.name;

    log.LogInformation($"{name}");

    if(name.ToLower().StartsWith('p')){
        followersCount+=100 ;
    }
    // We can implement some complex logic here. For the sake of simplicity, 
    //we add 100 to the followersCount if the name of the user starts with P
    //Otherwise,just return the same value which is recieved 

    return (ActionResult)new OkObjectResult(followersCount);
       
}
