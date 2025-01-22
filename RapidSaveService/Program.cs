using ARMCommon.Helpers;
using ARMCommon.Helpers.RabbitMq;
using ARMCommon.Interface;
using ARMCommon.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace ARMServices
{
    class Program
    {
        static void Main(string[] args)
        {

            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var json = File.ReadAllText(appSettingsPath);
            dynamic obj = JsonConvert.DeserializeObject(json);
            string queueName = obj.AppConfig["QueueName"];
            string signalrUrl = obj.AppConfig["SignalRURL"];
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();
            serviceCollection.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var rabbitMQConsumer = serviceProvider.GetService<IRabbitMQConsumer>();
            var rabbitMQProducer = serviceProvider.GetService<IRabbitMQProducer>();

            async Task<string> OnConsuming(string message)
            {
                try
                {

                    Console.WriteLine("OnConsuming method called with message " + message);

                    // Parse the incoming JSON message
                    JObject saveObj = JObject.Parse(message);
                    string queueData = string.Empty;
                    string armResponseQueue = string.Empty;
                    string axResponseQueue = string.Empty;
                    string url = string.Empty;
                    string method = string.Empty;
                    JObject saveData;
                    queueData = GetTokenIgnoreCase(saveObj, "queuedata")?.ToString();
                    JObject queueDataObj = JObject.Parse(queueData);



                    if (queueData != null)
                    {
                        string queueDataEscape = queueData.Replace("\r\n", "").Trim('"');
                        saveData = JsonConvert.DeserializeObject<JObject>(queueDataEscape);
                    }
                    else
                    {
                        saveData = saveObj;
                    }

                    JObject rapidsave = (JObject)saveData["payload"]?["rapidsave"];

                    if (rapidsave == null)
                    {
                        Console.WriteLine("rapidsave object missing.");
                        return string.Empty;
                    }


                    string impFileWithPath = 
                        rapidsave["impfilewithpath"]?.ToString();
                    string impFileWithSummary = rapidsave["impfilewithsummary"]?.ToString();
                    string project = saveData["payload"]?["rapidsave"]?["project"]?.ToString();
                    string apiDesc = saveData["payload"]?["rapidsave"]?["apidesc"]?.ToString();

                    JObject resultJson = new JObject
                    {
                        ["rapidsave"] = new JObject
                        {
                            ["project"] = rapidsave["project"],
                            ["userauthkey"] = rapidsave["userauthkey"],
                            ["token"] = rapidsave["token"],
                            ["seed"] = rapidsave["seed"],
                            ["transid"] = rapidsave["transid"],
                            ["keyfield"] = rapidsave["keyfield"],
                            ["returnfailedrecords"] = rapidsave["returnfailedrecords"],
                            ["trace"] = rapidsave["trace"],
                            ["impfilename"] = rapidsave["impfilename"],
                            ["impfilewithpath"] = impFileWithPath,
                            ["impfilewithsummary"] = impFileWithSummary,
                            ["impreccount"] = rapidsave["impreccount"],
                            ["isxlimport"] = rapidsave["isxlimport"],
                            ["dataarray"] = rapidsave["dataarray"],
                            ["impinitiatedon"] = rapidsave["impinitiatedon"],
                            ["xlimportbatchid"] = rapidsave["xlimportbatchid"],
                            ["xlimportbatchcount"] = rapidsave["xlimportbatchcount"],
                            ["xlimportbatchtotal"] = rapidsave["xlimportbatchtotal"],
                            
                        }
                    };

                    string payload = JsonConvert.SerializeObject(resultJson, Formatting.Indented);
                    string mediaType = "application/json";
                    API _api = new API();
                    try
                    {
                        ApiRequest apiRequest = new ApiRequest();
                        apiRequest.StartTime = DateTime.Now;
                        if (string.IsNullOrEmpty(obj.AppConfig?["URL"]?.ToString()))
                        {
                            url = obj.AppConfig["URL"];
                            method = obj.AppConfig["METHOD"];
                        }

                        if (string.IsNullOrEmpty(url))
                        {
                            url = queueDataObj["url"]?.ToString();
                            method = queueDataObj["method"]?.ToString();
                        }

                        ARMResult apiResult;
                        if (method.ToUpper() == "POST")
                        {
                            Console.WriteLine("url called: " + url);
                            Console.WriteLine("payload called: " + payload);
                            apiResult = await _api.POSTData(url, payload, mediaType);

                        }
                        else
                        {
                            Console.WriteLine("url called: " + url);
                            Console.WriteLine("payload called: " + payload);
                            apiResult = await _api.GetData(url);
                        }

                        var result = JsonConvert.SerializeObject(ParseAxpertRestAPIResult(apiResult));
                        if (!string.IsNullOrEmpty(armResponseQueue))
                        {
                            WriteMessage(" sending message to armresponsequeue" + armResponseQueue);
                            Console.WriteLine("payload called" + payload);
                            rabbitMQProducer.SendMessages(result, armResponseQueue);
                        }
                        if (!string.IsNullOrEmpty(axResponseQueue))
                        {
                            Console.WriteLine(" sending message to axresponsequeue" + axResponseQueue);
                            rabbitMQProducer.SendMessages(result, axResponseQueue);
                        }

                        WriteMessage(result);
                        apiRequest.Project = project;
                        apiRequest.Url = url;
                        apiRequest.Method = method;
                        apiRequest.RequestString = JsonConvert.SerializeObject(new { submitdata = payload });
                        if (Convert.ToBoolean(apiResult.result["success"]))
                        {
                            apiRequest.Status = "Success";
                        }
                        else
                        {
                            apiRequest.Status = "Fail";
                        }
                        apiRequest.Response = apiResult.result["message"].ToString();
                        apiRequest.EndTime = DateTime.Now;
                        apiRequest.APIDesc = apiDesc;

                        await LogAPICall(apiRequest);
                        //Task logResult = LogAPICall(apiRequest);
                        //Task.WaitAll(logResult);
                        return result;
                    }
                    catch (Exception ex)
                    {
                        var errResult = JsonConvert.SerializeObject(ex);
                        WriteMessage(errResult);
                        return errResult;
                    }

                }
                catch (Exception ex)
                {
                    var errResult = JsonConvert.SerializeObject(ex);
                    WriteMessage(errResult);
                    return ex.Message;
                }

            }

            static void WriteMessage(string message)
            {
                Console.WriteLine(DateTime.Now.ToString() + " - " + message);
            }

            static JToken GetTokenIgnoreCase(JObject jObject, string propertyName)
            {
                // Find the property in a case-insensitive manner
                var property = jObject.Properties()
                    .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

                return property?.Value;
            }

            static void RemoveJsonKey(ref JObject jsonObject, string key)
            {
                foreach (var property in jsonObject.Properties())
                {
                    if (property.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        jsonObject.Remove(property.Name);
                        break;
                    }
                }
            }

            static void UpdateOrAddJsonKey(ref JObject jsonObject, string key, JToken value)
            {
                bool keyExists = false;

                foreach (var property in jsonObject.Properties())
                {
                    if (property.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        property.Value = value;
                        keyExists = true;
                        break;
                    }
                }

                if (!keyExists)
                {
                    jsonObject[key] = value;
                }
            }

            static APIResult ParseAxpertRestAPIResult(ARMResult armResult)
            {
                APIResult result = new APIResult();
                result.data.Add("success", armResult.result["success"]);
                JObject resultJsonObj = JObject.Parse(armResult.result["message"].ToString());
                if (!(resultJsonObj["status"]?.ToString().ToLower() == "true" || resultJsonObj["status"]?.ToString().ToLower() == "success"))
                {
                    result.error = resultJsonObj["result"].ToString();
                    return result;
                }
                foreach (var property in resultJsonObj.Properties())
                {
                    if (property.Name.ToLower() != "status")
                        result.data.Add(property.Name, property.Value);
                }
                return result;

            }

            async Task<bool> LogAPICall(ApiRequest apiRequest)
            {
                if (string.IsNullOrEmpty(apiRequest.Project))
                {
                    Console.WriteLine("Project details is missing in Json. Can't write logs to 'axapijobdetails' table.");
                    return false;
                }

                var context = new ARMCommon.Helpers.DataContext(configuration);
                var redis = new RedisHelper(configuration);
                Utils utils = new Utils(configuration, context, redis);

                try
                {
                    Dictionary<string, string> config = await utils.GetDBConfigurations(apiRequest.Project);
                    string connectionString = config["ConnectionString"];
                    string dbType = config["DBType"];
                    string sql = Constants_SQL.INSERT_TO_APILOG;
                    string currentTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    sql = string.Format(sql, currentTime, currentTime, apiRequest.Url, apiRequest.Method, (apiRequest.RequestString), (apiRequest.ParameterString), (apiRequest.HeaderString), (apiRequest.Response), apiRequest.Status, apiRequest.StartTime?.ToString("yyyy-MM-dd HH:mm:ss.fff"), apiRequest.EndTime?.ToString("yyyy-MM-dd HH:mm:ss.fff"), "RMQ", (apiRequest.APIDesc ?? "ARMRapidSaveService"));
                    IDbHelper dbHelper = DBHelper.CreateDbHelper(dbType);
                    var result = await dbHelper.ExecuteQueryAsync(sql, connectionString);
                    //WriteMessage($"API log is done");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error:" + ex.Message, ex.StackTrace);
                }
                return true;
            }


            rabbitMQConsumer.DoConsume(queueName, OnConsuming);
        }
    }

    public class APIResult
    {
        public string error { get; set; }
        public Dictionary<string, object> data = new Dictionary<string, object>();
    }
}
