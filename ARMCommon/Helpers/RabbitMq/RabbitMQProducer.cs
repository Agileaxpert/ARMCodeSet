using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using System.Diagnostics;

namespace ARMCommon.Helpers
{
    public class RabbitMQProducer : IRabbitMQProducer
    {
        private readonly IConfiguration _configuration;

        public RabbitMQProducer(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public bool SendMessage<T>(T message, string queueName)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_configuration["AppConfig:RMQIP"])
                };

                var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: "logQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);
   
                channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
                channel.ConfirmSelect();
                return true;
            }
            catch (Exception ex)
            {
                LogEvent("Error" + ex.Message + ex.StackTrace + ex.InnerException);
                Console.WriteLine(JsonConvert.SerializeObject(ex.Message));
                return false;
            }
        }

        public bool SendMessages<T>(T message, string queueName, bool trace = false, int delayTimeInMs = 0)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_configuration["AppConfig:RMQIP"])
                };

                bool delayedmessage = true;// _configuration.GetValue<bool>("delayedmessage");
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    if (delayedmessage)
                    {
                        IDictionary<string, object> args = new Dictionary<string, object>
                {
                    {"x-delayed-type", "direct"}
                };

                        channel.ExchangeDeclare($"{queueName}-exchange", "x-delayed-message", true, false, args);
                    }
                    else
                    {
                        channel.ExchangeDeclare($"{queueName}-exchange", ExchangeType.Direct, true, false, null);
                    }

                    var queue = channel.QueueDeclare(queueName, true, false, false, null);
                    channel.QueueBind(queue, $"{queueName}-exchange", $"{queueName}-route");

                    var props = channel.CreateBasicProperties();
                    props.Headers = new Dictionary<string, object>
            {
                {"x-delay", delayTimeInMs},
                {"trace", trace}
            };
                    trace = true;
                    if (trace)
                    {
                        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QueueLogs.txt");
                        StreamWriter sw = new StreamWriter(path, true);
                        var messages = "Producer is Sending  Message   " + DateTime.Now.ToString() + message.ToString();
                        sw.WriteLine(messages);
                        sw.Close();
                    }

                    channel.BasicPublish($"{queueName}-exchange", $"{queueName}-route", props, Encoding.Default.GetBytes(message.ToString()));
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogEvent("Error" + ex.Message + ex.StackTrace + ex.InnerException);
                Console.WriteLine(ex.ToString());
                return false;
            }
        }



        private void LogEvent(string message) {
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry(message, EventLogEntryType.Information, 101, 1);

            }
        }
    }
}
