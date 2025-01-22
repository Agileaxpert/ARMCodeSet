using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ARMCommon.Helpers.RabbitMq
{

    public class RabbitMQConsumer : IRabbitMQConsumer
    {
        private readonly IConfiguration _configuration;
        public delegate Task<string> MyDelegate(string message);
        public RabbitMQConsumer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void DoConsume(string queueName, MyDelegate OnConsume)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_configuration["AppConfig:RMQIP"])
            };

            bool delayedmessage = true;
            var connection = factory.CreateConnection();

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
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var data = new EventArgsData
                    {
                        ExchangeName = ea.Exchange,
                        RoutingKey = ea.RoutingKey,
                        Headers = ea.BasicProperties.Headers,
                        Body = ea.Body.ToArray(),
                        Message = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray())
                    };
                    var headers = ea.BasicProperties.Headers;
                    var Body = ea.Body.ToArray();
                    string message = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine(" consumer Received message from producer: {0}", DateTime.Now.ToString());
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QueueLogs.txt");
                    bool traceValue = true;//headers.ContainsKey("trace") ? Convert.ToBoolean(headers["trace"].ToString()) : false;
                    if (traceValue)
                    {
                        StreamWriter sw = new StreamWriter(path, true);
                        var messages = " Traces of Consumer Message are  " + DateTime.Now.ToString() + message;
                        sw.WriteLine(messages);
                        sw.Close();
                    }
                    OnConsume(data.Message);

                };
                channel.BasicConsume(queueName, true, consumer);
                Console.WriteLine($"Connected to {queueName}.");
                Console.ReadLine();
            }
        }

        public void DeleteQueue(string queueName)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_configuration["AppConfig:RMQIP"])
            };

            var connection = factory.CreateConnection();

            using (var channel = connection.CreateModel())
            {
                channel.QueueDelete(queueName);
            }
        }

    }
}

