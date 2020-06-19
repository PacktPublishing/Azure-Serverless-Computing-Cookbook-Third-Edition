using System.Net;
using Microsoft.AspNetCore.Mvc;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    return (ActionResult)new OkObjectResult($"Hello User! Thanks for keeping me Warm");
}
