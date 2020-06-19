#r "../bin/Reusability.dll"
using Utilities;
public static async Task Run(HttpRequest req, ILogger log)
{
   log.LogInformation(Helper.GetReusableFunctionOutput());
}
