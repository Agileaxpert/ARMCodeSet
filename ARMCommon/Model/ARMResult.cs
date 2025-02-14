

using Newtonsoft.Json.Linq;
using System.Data;

namespace ARMCommon.Model
{
    public class ARMResult
    {
        //public object? result { get; set; }


        //public ARMResult()
        //{
        //}

        //public ARMResult(bool success = false, string message = ""){
        //    result = new
        //    {
        //        success = success,
        //        message = message
        //    };
        //}

        //public ARMResult(bool success = false, string message = "", string data = "")
        //{
        //    var resultData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
        //    if (resultData == null) 
        //        resultData = new Dictionary<string, object>();
        //    resultData.Add("success", success);
        //    if(!string.IsNullOrEmpty(message)) 
        //        resultData.Add("message", message);
        //    result = resultData;
        //}
        public Dictionary<string, object> result = new Dictionary<string, object>();
        public JObject Result;

        public ARMResult()
        {
        }

        public ARMResult(bool success = false, string message = "")
        {
            result.Add("success", success);
            result.Add("message", message);
        }

    }

    public class SQLResult
    {
        public string error { get; set; }
        public DataTable data { get; set; }
        public bool success { get; set; }
        public SQLResult()
        {
            data = new DataTable();
        }
    }


    public class SQLDataSetResult
    {
        public string error { get; set; }
        public DataSet DataSet { get; set; }
        public SQLDataSetResult()
        {
            DataSet = new DataSet();
        }
    }
    public class APIResult
    {
        public string error { get; set; }
        public Dictionary<string, object> data = new Dictionary<string, object>();
        public string message { get; set; }
    }

    public class NonQueryResult
    {
        public string error { get; set; }
        public string message { get; set; }
        public int count { get; set; }
    }
}
