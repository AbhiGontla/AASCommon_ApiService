using AASCommonApp_ApiService.Common;
using Microsoft.AnalysisServices.AdomdClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;


namespace AASCommonApp_ApiService.Controllers
{
    //[RoutePrefix("Query")]
    public class HomeController : ApiController
    {
        #region GetVehicles_FW 
        [Route("GetVehicles_FW")]
        [HttpGet]
        public Task<string> ReadFromAzureAS()
        {
            // var query = @"Evaluate TOPN(10,DimDate,DimDate[Date],1)";
            string query = @"Evaluate ('VEHICLES_FW')";
            return getData(query,100);
        }
        #endregion

        #region Parameters

        string domain = ConfigurationManager.AppSettings["domain"];
        string ssasUrl = ConfigurationManager.AppSettings["ssasUrl"];
        string clientId = ConfigurationManager.AppSettings["clientId"];
        string clientSecret = ConfigurationManager.AppSettings["clientSecret"];

        #endregion

        #region PowerBI Cards

        #region TotalCrewLeaders 
        [Route("TotalCrewLeaders")]
        [HttpGet]
        public Task<string> Total_Crew_Leaders()
        {
            string query = "EVALUATE(ROW(\"TotalCrewLeaders\",TBL_T_TSDetails[Total Crew Leaders New]))";
           
            
            return getData(query,1);
        }
        #endregion

        #region TotalCrewMembers
        [Route("TotalCrewMembers")]
        [HttpGet]
        public Task<string> Total_Crew_Members()
        {
            string query = "Evaluate (ROW(\"Total Crew Members\",TBL_T_TSDetails[Total Crew Members New]))";
            return getData(query,2);
        }
        #endregion

        #region TotalEquipments
        [Route("TotalEquipments")]
        [HttpGet]
        public Task<string> Total_Equipments()
        {
            string query = "Evaluate (ROW(\"Total Equipments\",TBL_T_TSEquipmentDetail[Total Equipments New]))";
            return getData(query,3);
        }
        #endregion

        #region TotalRegularHours
        [Route("TotalRegularHours")]
        [HttpGet]
        public Task<string> Total_Regular_Hours()
        {
            string query = "Evaluate (ROW(\"Total Regular Hours\",TBL_T_TSDetails[Total Regular Hours New]))";
            return getData(query,4);
        }
        #endregion

        #region TotalOvertimeHours
        [Route("TotalOvertimeHours")]
        [HttpGet]
        public Task<string> Total_Overtime_Hours()
        {
            string query = "Evaluate (ROW(\"Total Overtime Hours\",TBL_T_TSDetails[Total Overtime Hours New]))";
            return getData(query,5);
        }
        #endregion

        #endregion

        #region Reports


        #region Crew Leader Details

        [Route("CrewLeaderDetails")]
        [HttpGet]
        public Task<string> Crew_Leader_Details()
        {
            string query = "Evaluate (SUMMARIZECOLUMNS(TBL_T_TSCrew_RP[EmployeeName],CALCULATETABLE(TBL_T_TSCrew_RP),\"Total Regular Hours\",TBL_T_TSDetails[Total Regular Hours New],\"Total Overtime Hours\",[Total Overtime Hours New]))order by(TBL_T_TSCrew_RP[EmployeeName])";
            return getData(query,6);
        }
        #endregion

        #region Crew Member Details

        [Route("CrewMemberDetails")]
        [HttpGet]
        public Task<string> Crew_Member_Details()
        {
            string query = "EVALUATE (TOPN (25000, SUMMARIZE(CALCULATETABLE (TBL_T_TSCrew,TBL_T_TSCrew_RP,TBL_T_TSDetails,TBL_T_TSHeader,TBL_M_EMPLOYEE_HIERARCHY),TBL_T_TSCrew[EmployeeName],TBL_T_TSHeader[JobNumber],\"Total Regular Hours\", TBL_T_TSDetails[Total Regular Hours New],\"Total Overtime Hours\", TBL_T_TSDetails[Total Overtime Hours New])))ORDER BY ( TBL_T_TSCrew[EmployeeName] )";
            return getData(query,7);
        }

        #endregion

        #endregion

        #region GetConncetionString     
        public string GetConn(string ssal, string _pass)
        {
            string clientSecret = ConfigurationManager.AppSettings["connectionString"];
            string conn = String.Format(clientSecret, ssal, _pass);
            return conn;
        }
        #endregion

        #region CommonMethod

        public async Task<string> getData(string query,int id)
        {
            try
            {
                var token = await ADALTokenHelper.GetAppOnlyAccessToken(domain, $"https://{ssasUrl}", clientId, clientSecret);
                var connectionString = GetConn(ssasUrl, token);
                var ssasConnection = new AdomdConnection(connectionString);
                ssasConnection.Open();
                //var cmd = new AdomdCommand(query)
                //{
                //    Connection = ssasConnection
                //};

                DataTable dt = new DataTable();


                using (AdomdCommand cmd = new AdomdCommand(query, ssasConnection))
                {

                    AdomdDataAdapter da = new AdomdDataAdapter(cmd);

                    da.Fill(dt);

                    //return dt;

                }
                ssasConnection.Close();
                switch (id)
                {
                    case 1:
                        dt.Columns[0].ColumnName = "TotalCrewLeaders";
                        break;
                    case 2:
                        dt.Columns[0].ColumnName = "TotalCrewMembers";
                        break;
                    case 3:
                        dt.Columns[0].ColumnName = "TotalEquipments";
                        break;
                    case 4:
                        dt.Columns[0].ColumnName = "TotalRegularHours";
                        break;
                    case 5:
                        dt.Columns[0].ColumnName = "TotalOvertimeHours";
                        break;
                    case 6:
                        dt.Columns[0].ColumnName = "Employee Name";
                        dt.Columns[1].ColumnName = "TotalRegularHours";
                        dt.Columns[2].ColumnName = "TotalOvertimeHours";
                        break;

                    case 7:
                        dt.Columns[0].ColumnName = "Employee Name";
                        dt.Columns[1].ColumnName = "Job Number";
                        dt.Columns[2].ColumnName = "TotalRegularHours";
                        dt.Columns[3].ColumnName = "TotalOvertimeHours";
                        break;
                }

                string JsonResult;
                JsonResult = JsonConvert.SerializeObject(dt);
             //   var yourString = JsonResult.Replace('{\', '{');
                return JsonResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
