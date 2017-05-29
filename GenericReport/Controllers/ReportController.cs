using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using GenericReport.Helpers;

namespace GenericReport.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TestReport()
        {
            //This is just to create the connection string.
            //Normally it would be in the web config.
            string constr = new SqlConnectionStringBuilder()
            {
                DataSource = @"JERROD-PC\VOLZKA_DATA",
                IntegratedSecurity = true,
                InitialCatalog = "TestPeople"
            }.ConnectionString;

            //This is the base call. 
            //It consists of 3 parameters
            // 1)The connection string to the database you will get the data from
            // 2)The name of the stored procedure to get the data from
            // 3)A list of sql parameters used to get the report data.
            //Since my test report needed no parameters, they were not included in the call. 
            //See the AdvanceReport action for a demo.
            ReportData data = new ReportData(constr, "GetPersonReport");

            string test = data.ToCSV();

            return View(data);
        }

        public ActionResult AdvancedReport()
        {
            string constr = new SqlConnectionStringBuilder()
            {
                DataSource = @"JERROD-PC\VOLZKA_DATA",
                IntegratedSecurity = true,
                InitialCatalog = "TestPeople"
            }.ConnectionString;

            //Normally you would get the parameters from the request.
            List<SqlParameter> pars = new List<SqlParameter>()
            {
                new SqlParameter("@DOBStartDate", new DateTime(1980,1,1))
            };

            return View("TestReport", new ReportData(constr, "GetPersonReport", pars));
        }
    }
}