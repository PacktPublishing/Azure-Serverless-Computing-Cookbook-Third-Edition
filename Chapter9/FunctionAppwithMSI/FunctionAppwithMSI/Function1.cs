using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Azure.Services.AppAuthentication;

namespace FunctionAppwithMSI
{
    public static class HttpTriggerWithMSI
    {
        [FunctionName("HttpTriggerWithMSI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a  request.");


            string firstname = null,
              lastname = null, email = null;
          

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
           
            firstname = firstname ?? data?.firstname;
            lastname = lastname ?? data?.lastname;
            email = email ?? data?.email;
            //var devicelist = data.devicelist;

            SqlConnection con = null; 
            try
            {
                
                string query = "INSERT INTO EmployeeInfo (firstname,lastname, email) " + "VALUES (@firstname,@lastname, @email) ";

                con = new SqlConnection("Server=tcp:<dbservername>.database.windows.net,1433;Initial Catalog=cookbookdatabase;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                
                SqlCommand cmd = new SqlCommand(query, con);

                con.AccessToken = (new AzureServiceTokenProvider()).GetAccessTokenAsync("https://database.windows.net/").Result;

                cmd.Parameters.Add("@firstname", SqlDbType.VarChar, 50).Value = firstname;

                cmd.Parameters.Add("@lastname", SqlDbType.VarChar,50).Value = lastname;
                cmd.Parameters.Add("@email", SqlDbType.VarChar, 50).Value = email; 
                
                con.Open(); 
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw ex;
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
            }
            return new OkObjectResult("Hello, Successfully inserted the data");

        }
    }
}
