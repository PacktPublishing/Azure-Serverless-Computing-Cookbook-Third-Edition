using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace FunctionAppinVisualStudio
{
    public class FeedAIWithCustomDerivedMetrics
    {
        private const string AppInsightsApi = "https://api.applicationinsights.io/beta/apps";

        
        private static readonly TelemetryClient TelemetryClient = new TelemetryClient { InstrumentationKey = Environment.GetEnvironmentVariable("AI_IKEY") };
        private static readonly string AiAppId = Environment.GetEnvironmentVariable("AI_APP_ID");
        private static readonly string AiAppKey = Environment.GetEnvironmentVariable("AI_APP_KEY");

        [FunctionName("FeedAIWithCustomDerivedMetrics")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            await ScheduledAnalyticsRun(
                name: "Request per hour",
                query: @"requests | where timestamp > now(-1h)| summarize count()", 
                log: log);
        }
        public static async Task ScheduledAnalyticsRun(string name, string query, ILogger log)
        {
            log.LogInformation($"Executing scheduled analytics run for {name} at: {DateTime.Now}");

          
            string requestId = Guid.NewGuid().ToString();
            log.LogInformation($"[Verbose]: API request ID is {requestId}");

            try
            {
                MetricTelemetry metric = new MetricTelemetry { Name = name };
                metric.Context.Operation.Id = requestId;
                metric.Properties.Add("TestAppId", AiAppId);
                metric.Properties.Add("TestQuery", query);
                metric.Properties.Add("TestRequestId", requestId);
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("x-api-key", AiAppKey);
                    httpClient.DefaultRequestHeaders.Add("x-ms-app", "FunctionTemplate");
                    httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", requestId);
                    string apiPath = $"{AppInsightsApi}/{AiAppId}/query?clientId={requestId}&timespan=P1D&query={query}";
                    using (var httpResponse = await httpClient.GetAsync(apiPath))
                    {
                      
                        httpResponse.EnsureSuccessStatusCode();
                        var resultJson = await httpResponse.Content.ReadAsAsync<JToken>();
                        double result;
                        if (double.TryParse(resultJson.SelectToken("Tables[0].Rows[0][0]")?.ToString(), out result))
                        {
                            metric.Sum = result;
                            log.LogInformation($"[Verbose]: Metric result is {metric.Sum}");
                        }
                        else
                        {
                            log.LogError($"[Error]: {resultJson.ToString()}");
                            throw new FormatException("Query must result in a single metric number. Try it on Analytics before scheduling.");
                        }
                    }
                }

                TelemetryClient.TrackMetric(metric);
                log.LogInformation($"Metric telemetry for {name} is sent.");
            }
            catch (Exception ex)
            {
              
                var exceptionTelemetry = new ExceptionTelemetry(ex);
                exceptionTelemetry.Context.Operation.Id = requestId;
                exceptionTelemetry.Properties.Add("TestName", name);
                exceptionTelemetry.Properties.Add("TestAppId", AiAppId);
                exceptionTelemetry.Properties.Add("TestQuery", query);
                exceptionTelemetry.Properties.Add("TestRequestId", requestId);
                TelemetryClient.TrackException(exceptionTelemetry);
                log.LogError($"[Error]: Client Request ID {requestId}: {ex.Message}");

               
                throw;
            }
            finally
            {
                TelemetryClient.Flush();
            }
        }
    }
}

