using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using System.Net.Http;
namespace FunctionAppinVisualStudio
{
    public static class ApplicationInsightsScheduledDigest
    {
        private const string AppInsightsApi = "https://api.applicationinsights.io/v1/apps";
        private static readonly string AiAppId = Environment.GetEnvironmentVariable("AI_APP_ID");
        private static readonly string AiAppKey = Environment.GetEnvironmentVariable("AI_APP_KEY");
        private static readonly string SendGridAPIKey = Environment.GetEnvironmentVariable("SendGridAPIKey");
       
        [FunctionName("ApplicationInsightsScheduledDigest")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string appName = "Azure Serverless Computing Cookbook";

            var today = DateTime.Today.ToShortDateString();

            DigestResult result = await ScheduledDigestRun(
                query: GetQueryString(),
                log: log
            );

            SendGridMessage message = new SendGridMessage();
            message.SetFrom(new EmailAddress("donotreply@example.com"));
            message.AddTo("prawin2k@gmail.com");
            message.SetSubject($"Your daily Application Insights digest report for {today}");
            var msgContent = GetHtmlContentValue("Azure Serverless Computing Cookbook", today, result);
            message.AddContent("text/html", msgContent);
            var client = new SendGridClient(SendGridAPIKey);
            var response = await client.SendEmailAsync(message);

            log.LogInformation($"Generating daily report for {today} at {DateTime.Now}");

        }

        static string GetHtmlContentValue(string appName, string today, DigestResult result)
        {
            return $@"
                <html><body>
                <p style='text-align: center;'><strong>{appName} daily telemetry report {today}</strong></p>
                <p style='text-align: center;'>The following data shows insights based on telemetry from last 24 hours.</p>
                <table align='center' style='width: 95%; max-width: 480px;'><tbody>
                <tr>
                <td style='min-width: 150px; text-align: left;'><strong>Total requests</strong></td>
                <td style='min-width: 100px; text-align: right;'><strong>{result.TotalRequests}</strong></td>
                </tr>
                <tr>
                <td style='min-width: 150px; text-align: left;'><strong>Failed requests</strong></td>
                <td style='min-width: 100px; text-align: right;'><strong>{result.FailedRequests}</strong></td>
                </tr>
                <td style='min-width: 150px; text-align: left;'><strong>Total exceptions</strong></td>
                <td style='min-width: 100px; text-align: right;'><strong>{result.TotalExceptions}</strong></td>
                </tr>
                </tbody></table>
                </body></html>
                ";
        }
        private static async Task<DigestResult> ScheduledDigestRun(string query, ILogger log)
        {
            log.LogInformation($"Executing scheduled daily digest run at: {DateTime.Now}");
            string requestId = Guid.NewGuid().ToString();
            log.LogInformation($"API request ID is {requestId}");
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("x-api-key", AiAppKey);
                    httpClient.DefaultRequestHeaders.Add("x-ms-app", "FunctionTemplate");
                    httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", requestId);
                    string apiPath = $"{AppInsightsApi}/{AiAppId}/query?clientId={requestId}&timespan=P1W&query={query}";
                    using (var httpResponse = await httpClient.GetAsync(apiPath))
                    {
                        httpResponse.EnsureSuccessStatusCode();
                        var resultJson = await httpResponse.Content.ReadAsAsync<JToken>();
                        DigestResult result = new DigestResult
                        {
                            TotalRequests = resultJson.SelectToken("tables[0].rows[0][0]")?.ToObject<long>().ToString("N0"),
                            FailedRequests = resultJson.SelectToken("tables[0].rows[0][1]")?.ToObject<long>().ToString("N0"),
                            TotalExceptions = resultJson.SelectToken("tables[0].rows[0][2]")?.ToObject<long>().ToString("N0")
                        };
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"[Error]: Client Request ID {requestId}: {ex.Message}");
                throw;
            }
        }
        private static string GetQueryString()
        {
            return @"
            requests
            | where timestamp > ago(1d)
            | summarize Row = 1, TotalRequests = sum(itemCount), FailedRequests = sum(toint(success == 'False')),
                RequestsDuration = iff(isnan(avg(duration)), '------', tostring(toint(avg(duration) * 100) / 100.0))
            | join (
            exceptions
            | where timestamp > ago(1d)
            | summarize Row = 1, TotalExceptions = sum(itemCount)) on Row
            | project TotalRequests, FailedRequests,TotalExceptions
            ";
        }
    }
    struct DigestResult
    {
        public string TotalRequests;
        public string FailedRequests;
        public string TotalExceptions;
    }
}
