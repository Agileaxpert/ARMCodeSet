
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace ARMCommon.Helpers
{
    public class SaveData
    {
        private RabbitMQDef _rabbitMQDef;
        public SaveData(RabbitMQDef rabbitMQDef)
        {
            if (!rabbitMQDef.rmqConnection.IsOpen)
            {
                rabbitMQDef.rmqConnection = new ConnectionFactory { HostName = "localhost" }.CreateConnection();
                rabbitMQDef.rmqChannel = rabbitMQDef.rmqConnection.CreateModel();

                //var properties = rabbitMQDef.rmqChannel.CreateBasicProperties();
                //properties.Persistent = true;
                //rabbitMQDef.rmqProperties = properties;
            }

            if (rabbitMQDef.rmqChannel is null)
            {
                rabbitMQDef.rmqChannel = rabbitMQDef.rmqConnection.CreateModel();
            }

            _rabbitMQDef = rabbitMQDef;
        }

        public async Task<string> SaveToRMQAsync(SaveDataDef data, string type)
        {

            var channel = _rabbitMQDef.rmqChannel;
            channel.QueueDeclare(queue: "SaveDataAsyncQueue",
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            string message = JsonSerializer.Serialize<SaveDataDef>(data);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                                    routingKey: "SaveDataAsyncQueue",
                                    basicProperties: null,
                                    body: body);

            return "Added to Queue";
        }

        public async Task<string> InsertDataAsync(string data)
        {

            var channel = _rabbitMQDef.rmqChannel;
            channel.QueueDeclare(queue: "InsertDataAsyncQueue",
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            //string message = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            var body = Encoding.UTF8.GetBytes(data);

            channel.BasicPublish(exchange: "",
                                    routingKey: "InsertDataAsyncQueue",
                                    basicProperties: null,
                                    body: body);

            //await Task.Run(()=>channel.BasicPublish(exchange: "",
            //routingKey: "InsertDataAsyncQueue",
            //basicProperties: null,
            //body: body));

            return "Added to Queue";
        }

        public async Task<string> SaveToDataSyncQueueAsync(string syncAPI)
        {

            var channel = _rabbitMQDef.rmqChannel;
            channel.QueueDeclare(queue: "DataSyncQueue",
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            var body = Encoding.UTF8.GetBytes(syncAPI);

            channel.BasicPublish(exchange: "",
                                    routingKey: "DataSyncQueue",
                                    basicProperties: null,
                                    body: body);

            return "Added to Queue";
        }

        public async Task<string> SaveToDBAsync(SaveDataDef data, string type)
        {
            string message = JsonSerializer.Serialize<SaveDataDef>(data);
            var response = "";
            var rpcClient = new RpcClient(_rabbitMQDef);

            response = await rpcClient.SendAsync(message);
            return response.ToString();
        }
    }

    public struct SaveDataDef
    {
        public string formId { get; set; }
        public Dictionary<string, object> saveJson { get; set; }
        //public DataSources dataSource { get; set; }
        public string userId { get; set; }
        public bool isAsync { get; set; }

    }


    public class RpcClient
    {
        private IModel channel;
        private EventingBasicConsumer consumer;

        private const string requestQueueName = "SaveData_RequestQueue";
        private const string responseQueueName = "SaveData_ResponseQueue";
        private const string exchangeName = "RPC";
        private RabbitMQDef _rabbitMQDef;

        public RpcClient(RabbitMQDef rabbitMQDef)
        {
            _rabbitMQDef = rabbitMQDef;
            channel = _rabbitMQDef.rmqChannel;

            channel.ExchangeDeclare(exchange: exchangeName, type: "direct");

            channel.QueueDeclare(queue: requestQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueDeclare(queue: responseQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            channel.QueueBind(queue: requestQueueName, exchange: exchangeName, routingKey: requestQueueName);
            channel.QueueBind(queue: responseQueueName, exchange: exchangeName, routingKey: responseQueueName);

            consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;
            channel.BasicConsume(queue: responseQueueName, autoAck: true, consumer: consumer);
        }

        public Task<string> SendAsync(string message)
        {
            var tcs = new TaskCompletionSource<string>();
            var correlationId = Guid.NewGuid().ToString();

            _rabbitMQDef.pendingTasks[correlationId] = tcs;

            this.Publish(message, correlationId);

            var result = tcs.Task.Result;
            return Task.Run(() => result);
        }

        private void Publish(string message, string correlationId)
        {
            var props = channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = responseQueueName;
            props.Persistent = true;

            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: exchangeName, routingKey: requestQueueName, basicProperties: props, body: messageBytes);
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var correlationId = e.BasicProperties.CorrelationId;
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            PushResult(correlationId, message);
        }

        private void PushResult(string correlationId, string message)
        {
            _rabbitMQDef.pendingTasks.TryRemove(correlationId, out var tcs);
            if (tcs != null)
                tcs.SetResult(message);
        }
    }

    public class RabbitMQDef
    {
        public IConnection rmqConnection;
        public ConcurrentDictionary<string,
            TaskCompletionSource<string>> pendingTasks = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
        public IModel rmqChannel;
        private readonly IConfiguration _configuration;

        public RabbitMQDef(IConfiguration configuration)
        {
            _configuration = configuration;
            rmqConnection = new ConnectionFactory { HostName = _configuration["AppConfig:RMQIP"] }.CreateConnection();
            rmqChannel = rmqConnection.CreateModel();
        }
    }
}
