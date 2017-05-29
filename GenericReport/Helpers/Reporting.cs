using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Collections;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.Text;

namespace GenericReport.Helpers
{
    public class ReportData
    {
        /// <summary>
        /// The connection string used to create the connection to the Sql Database
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Any Sql Parameters needed to obtain the data
        /// </summary>
        public List<SqlParameter> Parameters { get; set; }

        /// <summary>
        /// The name of the stored procedure used to obtainthe data
        /// </summary>
        public string StoredProcedureName { get; set; }
        
        /// <summary>
        /// An iDictionary&lt;string,string&gt; representation of the resulting data
        /// <para>Note, DBNull Values have been converted to standard nulls</para>
        /// </summary>
        public List<List<KeyValuePair<string, string>>> Data { get; set; }
        
        public ReportBoolType BoolType { get; set; }
        public ReportDateType DateType { get; set; }

        //Private variables
        private SqlConnection Connection { get; set; }
        private SqlCommand Command { get; set; }
        private SqlDataReader Reader { get; set; }
        
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ReportData() { }

        /// <summary>
        /// Constructor: When this constructor is used, data will be obtained immediately.
        /// </summary>
        /// <param name="connectionString">Connection string to the SqlServer instance including database name</param>
        /// <param name="storedProcedureName">The stored procedure used to call get the data</param>
        /// <param name="parameters">Any SqlParametrs needed to obtainthe data</param>
        public ReportData(string connectionString, string storedProcedureName, List<SqlParameter> parameters = null, ReportBoolType boolType = ReportBoolType.YesNo, ReportDateType dateType = ReportDateType.ShortDateString)
        {
            ConnectionString = connectionString;
            StoredProcedureName = storedProcedureName;
            Parameters = parameters;
            DateType = dateType;
            BoolType = boolType;
            Data = new List<List<KeyValuePair<string, string>>>();
            GetData();
        }

        /// <summary>
        /// Uses the information currently provided to attempt to obtain data from the database.
        /// </summary>
        public void GetData()
        {            
            Data = new List<List<KeyValuePair<string, string>>>();

            using (Connection = new SqlConnection(ConnectionString))
            {
                Connection.Open();
                using (Command = new SqlCommand(StoredProcedureName, Connection))
                {
                    Command.CommandType = System.Data.CommandType.StoredProcedure;
                    if (Parameters != null)
                    {
                        Command.Parameters.AddRange(Parameters.ToArray());
                    }

                    using (Reader = Command.ExecuteReader())
                    {
                        if (Reader.HasRows)
                        {                            
                            while (Reader.Read())
                            {
                                List<KeyValuePair<string, string>> temp = new List<KeyValuePair<string, string>>();
                                int c = 0;
                                while (c < Reader.VisibleFieldCount)
                                {
                                    temp.Add(new KeyValuePair<string, string>(Reader.GetName(c), Reader.GetValue(c) != DBNull.Value ? Reader.GetValue(c).ToString() : ""));
                                    c++;
                                }
                                Data.Add(temp);                                
                            }
                        }
                    }
                }
            }
        }

        public string ToCSV(bool addHeaders = true)
        {
            StringBuilder sb = new StringBuilder();

            if (Data.Count > 0)
            {
                if (addHeaders)
                {
                    foreach (KeyValuePair<string, string> prop in Data[0])
                    {
                        sb.Append(string.Format("{0}{1}", prop.Key, Data[0].IndexOf(prop) == Data[0].Count - 1 ? System.Environment.NewLine : ","));
                    }
                }
                
                foreach (List<KeyValuePair<string, string>> record in Data)
                {
                    foreach (KeyValuePair<string, string> prop in record)
                    {
                        sb.Append(string.Format("\"{0}\"{1}", prop.FixDateValue(BoolType, DateType), Data[0].IndexOf(prop) == Data[0].Count - 1 ? System.Environment.NewLine : ","));
                    }
                }
            }            
            return sb.ToString();
        }
    }

    public enum ReportDateType
    {
        LongDateString,
        ShortDateString,
        LongTimeString,
        ShortTimeString
    }

    public enum ReportBoolType
    {
        TrueFalse,
        YesNo,
        HtmlChecked,
    }

    public static class ReportDataExtensions
    {
        public static string FixDateValue(this KeyValuePair<string, string> kvp, ReportBoolType boolType = ReportBoolType.YesNo, ReportDateType dateType = ReportDateType.ShortDateString, bool toUTC = false)
        {
            DateTime dt = new DateTime();

            //To avoid calling TryParse on as many values as possible, 
            //this will filter out anything that does not start with a number.
            if (!string.IsNullOrEmpty(kvp.Value) && Regex.IsMatch(kvp.Value.Substring(0, 1), @"^\d+$"))
            {
                if (DateTime.TryParse(kvp.Value, out dt))
                {
                    if (toUTC)
                    {
                        dt = dt.ToUniversalTime();
                    }

                    switch (dateType)
                    {
                        case ReportDateType.ShortDateString:
                            return dt.ToShortDateString();
                        case ReportDateType.LongDateString:
                            return dt.ToLongDateString();
                        case ReportDateType.ShortTimeString:
                            return dt.ToShortTimeString();
                        case ReportDateType.LongTimeString:
                            return dt.ToLongTimeString();
                    }
                }
            }
            else
            {
                bool bl = false;
                if (bool.TryParse(kvp.Value, out bl))
                {
                    switch (boolType)
                    {
                        case ReportBoolType.YesNo:
                            return bl ? "Yes" : "No";
                        case ReportBoolType.HtmlChecked:
                            string ckd = bl ?"checked=\"checked\"" : "";
                            return string.Format("<input type=\"checkbox\" {0} readonly=\"readonly\"/>", ckd);
                        default:
                            return kvp.Value;
                    }
                }
            }
            return kvp.Value;
        }
    }
}