using AASCommonApp_ApiService.Common;
using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AASCommonApp_ApiService.Controllers
{
    public class HomeController : ApiController
    {
    
        [Route("GetAzureData")]  
        [HttpGet]
        public  async Task<string> ReadFromAzureAS()
        {           
            var clientId = "ca229f71-547e-4ac9-9840-7fa39fe3ba72";
            var clientSecret = "wn6Q/z?P_xBjkedB5twme@gTK1VY1JNP";
            var domain = "pikeenterprises.onmicrosoft.com";

            var ssasUrl = "aspaaseastus2.asazure.windows.net"; //get this from your Azure AS connectionString

            var token = await ADALTokenHelper.GetAppOnlyAccessToken(domain, $"https://{ssasUrl}", clientId, clientSecret);

            var connectionString = $"Provider=MSOLAP;Data Source=asazure://{ssasUrl}/pikeedwdevaas;Initial Catalog= Pike_Supervisor;User ID=;Password={token};Persist Security Info=True;Impersonation Level=Impersonate";
            //var connectionString = $"Provider=MSOLAP;Data Source=asazure://{ssasUrl}/pikeedwdevaas;Initial Catalog= Pike_Supervisor;User ID=******;Password=******;Persist Security Info=True;Impersonation Level=Impersonate";

            var ssasConnection = new AdomdConnection(connectionString);
            ssasConnection.Open();
            // var query = @"Evaluate TOPN(10,DimDate,DimDate[Date],1)";
            var query = @"Evaluate ('VEHICLES_FW')";
            var cmd = new AdomdCommand(query)
            {
                Connection = ssasConnection
            };
            using (var reader = cmd.ExecuteXmlReader())
            {
                string value = reader.ReadOuterXml();
                return value;
            }
        }
    }
}
