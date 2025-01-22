using ARMCommon.Helpers;
using ARMCommon.Helpers.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PEGReminderService
{
    class Program
    {
        static void Main(string[] args)
        {

            var appSettingsPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "appsettings.json");
            var json = System.IO.File.ReadAllText(appSettingsPath);
            dynamic obj = JsonConvert.DeserializeObject(json);
            string queueName = obj.AppConfig["QueueName"];
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var rabbitMQConsumer = serviceProvider.GetService<IRabbitMQConsumer>();

            async Task<string> OnConsuming(string message)
            {
                try
                {
                    JObject messageData = JObject.Parse(message);
                    string queueData = messageData["queuedata"].ToString();
                    JObject messageDatas = JsonConvert.DeserializeObject<JObject>(queueData);
                    var payload = messageDatas["payload"].ToString().Replace("\r\n", "").Replace("\\", "");
                    string url = obj.AppConfig["URL"];
                    string method = "POST";

                    if (method == "POST")
                    {
                        string Mediatype = "application/json";
                        API _api = new API();
                        try
                        {
                            var apiResult = await _api.POSTData(url, payload, Mediatype);
                            Console.WriteLine(DateTime.Now + JsonConvert.SerializeObject(new { status = apiResult.result["success"], msg = apiResult.result["message"] }));
                            return JsonConvert.SerializeObject(new { status = apiResult.result["success"], msg = apiResult.result["message"] });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now + JsonConvert.SerializeObject(new { error = new List<string> { ex.Message } }));
                            return JsonConvert.SerializeObject(new { error = new List<string> { ex.Message } });
                        }
                    }

                    else
                    {
                        API _api = new API();
                        try
                        {
                            var apiResult = await _api.GetData(url);
                            return JsonConvert.SerializeObject(new { status = apiResult.result["success"], msg = apiResult.result["message"] });
                        }
                        catch (Exception ex)
                        {
                            return JsonConvert.SerializeObject(new { error = new List<string> { ex.Message } });
                        }
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }

            }


            rabbitMQConsumer.DoConsume(queueName, OnConsuming);
        }
    }

}