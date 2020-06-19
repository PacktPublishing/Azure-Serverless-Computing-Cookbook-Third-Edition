using System;

public async static void Run(TimerInfo myTimer, ILogger log)
{
   using (var httpClient = new HttpClient())
{
var response = await httpClient.GetAsync("https://azurefunctioncookbook-men.azurewebsites.net/api/HttpALive");
}

}
