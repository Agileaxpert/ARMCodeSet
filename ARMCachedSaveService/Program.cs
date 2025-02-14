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
            dynamic config = JsonConvert.DeserializeObject(json);
            string queueName = config.AppConfig["QueueName"];
            string signalrUrl = config.AppConfig["SignalRURL"];
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
                    JObject saveObj = JObject.Parse(message);
                    string queueData = string.Empty;
                    string armResponseQueue = string.Empty;
                    JObject saveData;
                    string SignalrClientId = saveObj["signalrclient"].ToString();
                    queueData = saveObj["queuedata"]?.ToString();
                    armResponseQueue = saveObj["responsequeue"]?.ToString();
                    if (queueData != null)
                    {
                        queueData = queueData.Replace("\r\n", "").Replace(@"\""", @"""").Trim('"');
                        saveData = JsonConvert.DeserializeObject<JObject>(queueData);
                        if (saveData?["payload"] != null)
                        {
                            saveData = (JObject)saveData["payload"];
                        }
                    }
                    else
                    {
                        saveData = saveObj;
                    }


                    string url = saveData["url"]?.ToString();
                    string method = saveData["method"]?.ToString();
                    string axResponseQueue = saveData["responsequeue"]?.ToString();
                    string transid = saveData["_parameters"][0]["savedata"]["transid"].ToString();
                    string project = saveData["_parameters"][0]["savedata"]["axpapp"].ToString();
                    string payload = JsonConvert.SerializeObject(saveData);
                    string mediaType = "application/json";
                    API _api = new API();
                    ApiRequest apiRequest = new ApiRequest();
                    apiRequest.StartTime = DateTime.Now;
                    apiRequest.Project = project;
                    apiRequest.Url = url;
                    apiRequest.Method = method;
                    apiRequest.RequestString = JsonConvert.SerializeObject(new { savedata = saveData });
                    apiRequest.APIDesc = "Cached Save";

                    try
                    {
                        if (string.IsNullOrEmpty(url))
                        {
                            url = config.AppConfig["URL"];
                            method = config.AppConfig["METHOD"];
                            apiRequest.Url = url;
                            apiRequest.Method = method;
                        }
                        ARMResult apiResult;
                        if (method == "POST")
                        {
                            Console.WriteLine("url called:" + url);
                            Console.WriteLine("payload called" + payload);
                            apiResult = await _api.POSTData(url, payload, mediaType);
                        }
                        else
                        {
                            Console.WriteLine("url called:" + url);
                            Console.WriteLine("payload called:" + payload);
                            apiResult = await _api.GetData(url);

                        }

                        if (!string.IsNullOrEmpty(SignalrClientId))
                        {
                            var successmessage = apiResult.result["message"] ?? "";

                            JObject apimessage = JObject.Parse(apiResult.result["message"].ToString());

                            // Accessing the "msg" and "recordid" nodes
                            string successapimessage = apimessage?["result"]?[0]?["message"]?[0]?["msg"]?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(successapimessage))
                            {

                                string recordid = apimessage?["result"]?[0]?["message"]?[0]?["recordid"]?.ToString() ?? "";

                                string link = "t" + transid + "&recordid=" + recordid;
                                var signalrobjlist = new List<object>
                            {
                                 new
                                 {
                                  type = "Tstruct Save Success",
                                  title = successapimessage,
                                  message = "",
                                  link = link
                                  }
                            };
                                Console.WriteLine("Message sent to signalR client is " + JsonConvert.SerializeObject(signalrobjlist));
                                var singalRMessage = new
                                {
                                    UserId = SignalrClientId,
                                    Message = JsonConvert.SerializeObject(signalrobjlist)
                                };
                                await _api.POSTData(signalrUrl, JsonConvert.SerializeObject(singalRMessage), "application/json");
                                Console.Write("message sent to signalR client");
                            }
                            else
                            {

                                string errormessage = apimessage["result"][0]["error"]["msg"].ToString() ?? "";
                                var signalrobjlist = new List<object>
                            {
                                 new
                                 {
                                  type = "Tstruct Save Failed",
                                  title = errormessage,
                                  message = "",
                                  link = ""
                                  }
                            };
                                Console.WriteLine("Message sent to signalR client is " + JsonConvert.SerializeObject(signalrobjlist));
                                var singalRMessage = new
                                {
                                    UserId = SignalrClientId,
                                    Message = JsonConvert.SerializeObject(signalrobjlist)
                                };
                                await _api.POSTData(signalrUrl, JsonConvert.SerializeObject(singalRMessage), "application/json");
                                Console.Write("message sent to signalR client");

                            }

                        }
                        var result = JsonConvert.SerializeObject(new { status = apiResult.result["success"], msg = apiResult.result["message"] });
                        if (!string.IsNullOrEmpty(armResponseQueue))
                        {
                            Console.WriteLine(" sending message to armresponsequeue" + armResponseQueue);
                            Console.WriteLine("payload called" + payload);
                            rabbitMQProducer.SendMessages(result, armResponseQueue);
                        }
                        if (!string.IsNullOrEmpty(axResponseQueue))
                        {
                            Console.WriteLine(" sending message to axresponsequeue" + axResponseQueue);
                            rabbitMQProducer.SendMessages(result, axResponseQueue);
                        }


                        WriteMessage(result);

                        if (Convert.ToBoolean(apiResult.result["success"]) && apiResult.result?["message"]?.ToString()?.IndexOf("\"error\"") == -1)
                        {
                            apiRequest.Status = "Success";
                        }
                        else
                        {
                            apiRequest.Status = "Fail";
                        }
                        apiRequest.Response = apiResult.result["message"].ToString();
                        apiRequest.EndTime = DateTime.Now;


                        await LogAPICall(apiRequest);
                        return result;
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            apiRequest.EndTime = DateTime.Now;
                            apiRequest.Status = "Fail";
                            apiRequest.Response = JsonConvert.SerializeObject(ex);
                            await LogAPICall(apiRequest);
                        }
                        catch (Exception ex1)
                        {
                            apiRequest.Response = JsonConvert.SerializeObject(ex1);
                            await LogAPICall(apiRequest);
                            var errResult1 = JsonConvert.SerializeObject(new { error = new List<string> { ex1.Message } });
                            Console.WriteLine(DateTime.Now + errResult1);
                        }

                        var errResult = JsonConvert.SerializeObject(new { error = new List<string> { ex.Message } });
                        Console.WriteLine(DateTime.Now + errResult);
                        return errResult;
                    }

                }
                catch (Exception ex)
                {
                    var errResult = JsonConvert.SerializeObject(new { error = new List<string> { ex.Message } });
                    Console.WriteLine(DateTime.Now + errResult);
                    return ex.Message;
                }

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
                    sql = string.Format(sql, currentTime, currentTime, apiRequest.Url, apiRequest.Method, (apiRequest.RequestString), (apiRequest.ParameterString), (apiRequest.HeaderString), (apiRequest.Response), apiRequest.Status, apiRequest.StartTime?.ToString("yyyy-MM-dd HH:mm:ss.fff"), apiRequest.EndTime?.ToString("yyyy-MM-dd HH:mm:ss.fff"), "RMQ", (apiRequest.APIDesc ?? "ARMCachedSaveService"));

                    IDbHelper dbHelper = DBHelper.CreateDbHelper(dbType);
                    var result = await dbHelper.ExecuteQueryAsync(sql, connectionString);
                    //WriteMessage($"API log is done. {JsonConvert.SerializeObject(result)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error:" + ex.Message);
                }
                return true;
            }

            static void WriteMessage(string message)
            {
                Console.WriteLine(DateTime.Now.ToString() + " - " + message);
            }


            rabbitMQConsumer.DoConsume(queueName, OnConsuming);
        }
    }

}
