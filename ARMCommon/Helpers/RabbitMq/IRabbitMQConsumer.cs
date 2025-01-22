

namespace ARMCommon.Helpers.RabbitMq
{
    public interface IRabbitMQConsumer
    {
        // delegate void MyDelegate(EventArgsData data);
        void DoConsume(string queueName, RabbitMQConsumer.MyDelegate function);
        void DeleteQueue(string queueName);
    }
}
