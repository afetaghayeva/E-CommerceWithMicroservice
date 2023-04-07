using EventBus.AzureServiceBus;
using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.RabbitMQ;

namespace EventBus.Factory
{
    public class EventBusFactory
    {
        public static IEventBus Create(EventBusConfig config, IServiceProvider provider)
        {
            return config.EventBusType switch
            {
                EventBusType.AzureService => new EventBusServiceBus( provider, config),
                _ => new EventBusRabbitMQ( provider, config)
            };
        }
    }
}