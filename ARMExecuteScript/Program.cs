using ARMCommon.Helpers;
using ARMCommon.Helpers.RabbitMq;
using ARMCommon.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace ARMExecuteScript
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var json = System.IO.File.ReadAllText(appSettingsPath);
            dynamic config = JsonConvert.DeserializeObject(json);
            string queueName = config.AppConfig["QueueName"];

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();
            serviceCollection.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();


            var serviceProvider = serviceCollection.BuildServiceProvider();
            var rabbitMQConsumer = serviceProvider.GetService<IRabbitMQConsumer>();
            var rabbitMQProducer = serviceProvider.GetService<IRabbitMQProducer>();


            async Task<string> OnConsuming(string message)
            {
                try
                {
                    WriteMessage("OnConsuming method called with message " + message);

                    JObject obje = JObject.Parse(message);
                    string armResponseQueue = string.Empty;
                    string queueData = GetTokenIgnoreCase(obje, "queuedata")?.ToString();

                    if (queueData != null)
                    {
                        JObject obj = JObject.Parse(queueData);
                        string Project = GetTokenIgnoreCase(obj, "project")?.ToString();
                        string Script = GetTokenIgnoreCase(obj, "script")?.ToString();
                        string Type = GetTokenIgnoreCase(obj, "type")?.ToString();
                        string Name = GetTokenIgnoreCase(obj, "name")?.ToString();
                        string RecordID = GetTokenIgnoreCase(obj, "recordid")?.ToString();
                        string Trace = GetTokenIgnoreCase(obj, "trace")?.ToString();
                        string UserName = GetTokenIgnoreCase(obj, "username")?.ToString();
                        string inputAPIURL = GetTokenIgnoreCase(obj, "apiurl")?.ToString();
                        string APIUrl = configuration["AppConfig:apiurl"];

                        AxpertRestAPIToken axpertRestAPIToken = new AxpertRestAPIToken(UserName);
                        var Seed = axpertRestAPIToken.seed.ToString();
                        var Token = axpertRestAPIToken.token.ToString();
                        var authkey = axpertRestAPIToken.userAuthKey.ToString();
                        var response = SubmitApi(APIUrl, Name, Project, Script, Type, RecordID, Trace, Token, Seed, authkey);
                    }

                    return "";

                }

                catch (Exception ex)
                {
                    var errResult = JsonConvert.SerializeObject(new { error = new List<string> { ex.Message } });
                    WriteMessage(errResult);
                    return ex.Message;
                }


            }


            static async Task<ApiResponse> SubmitApi(string APIUrl, string Name, string Project, string Script, string Type, string RecordID, string Trace, string Token, string Seed, string authkey)
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.ConnectionClose = true;
                    //client.DefaultRequestHeaders.Connection.Add("Keep-Alive");
                    var payload = new
                    {
                        executescript = new
                        {
                            project = Project,
                            token = Token,
                            seed = Seed,
                            userauthkey = authkey,
                            script = Script,
                            type = Type,
                            name = Name,
                            recordid = RecordID,
                            trace = Trace
                        }
                    };

                    var jsonPayload = JsonConvert.SerializeObject(payload);
                    Console.WriteLine($"Payload: {jsonPayload}");

                    var jsonContent = JsonConvert.SerializeObject(payload, Formatting.Indented);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(APIUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("API Result:" + responseContent.ToString());
                        return new ApiResponse(true, responseContent);

                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(errorContent.ToString());
                        return new ApiResponse(false, errorContent);
                    }
                }
            }

            static void WriteMessage(string message)
            {
                Console.WriteLine(DateTime.Now.ToString() + " - " + message);
            }

            static JToken GetTokenIgnoreCase(JObject jObject, string propertyName)
            {

                var property = jObject.Properties()
                    .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

                return property?.Value;
            }


            rabbitMQConsumer.DoConsume(queueName, OnConsuming);
        }
    }
}
public class ApiResponse
{
    public bool Success { get; }
    public string Content { get; }

    public ApiResponse(bool success, string content)
    {
        Success = success;
        Content = content;
    }
}