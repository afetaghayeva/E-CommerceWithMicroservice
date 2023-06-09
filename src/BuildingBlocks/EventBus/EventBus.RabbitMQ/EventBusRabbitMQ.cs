﻿using EventBus.Base;
using EventBus.Base.Events;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text;

namespace EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : BaseEventBus
    {
        private readonly RabbitMQPersistentConnection _persistentConnection;
        private readonly IConnectionFactory _connectionFactory;
        private IModel _consumerChannel;

        public EventBusRabbitMQ( IServiceProvider serviceProvider, EventBusConfig eventBusConfig) : base( serviceProvider, eventBusConfig)
        {
            if (base.EventBusConfig.Connection != null)
            {
                var connJson = JsonConvert.SerializeObject(base.EventBusConfig.Connection, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                });

                this._connectionFactory = JsonConvert.DeserializeObject<ConnectionFactory>(connJson);
            }
            else
                this._connectionFactory = new ConnectionFactory();

            this._persistentConnection = new RabbitMQPersistentConnection(this._connectionFactory, base.EventBusConfig.ConnectionRetryCount);

            this._consumerChannel = this.CreateConsumerChannel();

            base.SubsManager.OnEventRemoved += EventBusSubscriptionManager_OnEventRemove;
        }

        private void EventBusSubscriptionManager_OnEventRemove(object? sender, string eventName)
        {
            eventName = base.ProcessEventName(eventName);

            if (this._persistentConnection.ISConnected)
            {
                this._persistentConnection.TryConnect();
            }

            this._consumerChannel.QueueUnbind(queue: eventName, exchange: base.EventBusConfig.DefaultTopicName, routingKey: eventName);

            if (base.SubsManager.IsEmpty)
            {
                this._consumerChannel.Close();
            }
        }

        public override void Publish(IntegrationEvent @event)
        {
            if (!this._persistentConnection.ISConnected)
            {
                this._persistentConnection.TryConnect();
            }

            var policy = Policy.Handle<SocketException>().Or<BrokerUnreachableException>().WaitAndRetry(base.EventBusConfig.ConnectionRetryCount, retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)), (ex, time) =>
            {

            });

            var eventName = @event.GetType().Name;

            eventName = base.ProcessEventName(eventName);

            this._consumerChannel.ExchangeDeclare(exchange: base.EventBusConfig.DefaultTopicName, type: "direct");

            this._consumerChannel.QueueBind(queue: base.GetSubName(eventName),
                                                exchange: base.EventBusConfig.DefaultTopicName,
                                                routingKey: eventName);

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            policy.Execute(() =>
            {
                var properties = this._consumerChannel.CreateBasicProperties();
                properties.DeliveryMode = 2; //persistent


                //this._consumerChannel.QueueDeclare(queue: base.GetSubName(eventName), durable: true, exclusive: false, autoDelete: false, arguments: null);

                //this._consumerChannel.QueueBind(queue: base.GetSubName(eventName),
                //                                exchange: base.EventBusConfig.DefaultTopicName,
                //                                routingKey: eventName);

                this._consumerChannel.BasicPublish(exchange: base.EventBusConfig.DefaultTopicName,
                                                   routingKey: eventName,
                                                   mandatory: true,
                                                   basicProperties: properties,
                                                   body: body);
            });
        }

        public override void Subscribe<T, TH>()
        {
            var eventName = typeof(T).Name;
            _ = base.ProcessEventName(eventName);

            if (!base.SubsManager.HasSubscriptionsForEvent(eventName))
            {
                if (!this._persistentConnection.ISConnected)
                {
                    this._persistentConnection.TryConnect();
                }

                this._consumerChannel.QueueDeclare(queue: base.GetSubName(eventName), durable: true, exclusive: false, autoDelete: false, arguments: null);

                this._consumerChannel.QueueBind(queue: base.GetSubName(eventName),
                                                exchange: base.EventBusConfig.DefaultTopicName,
                                                routingKey: eventName);
            }

            base.SubsManager.AddSubscription<T, TH>();
            this.StartBasicConsume(eventName);
        }

        public override void UnSubscribe<T, TH>()
        {
            base.SubsManager.RemoveSubscription<T, TH>();
        }

        private IModel CreateConsumerChannel()
        {
            if (!this._persistentConnection.ISConnected)
            {
                this._persistentConnection.TryConnect();
            }

            var channel = this._persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: base.EventBusConfig.DefaultTopicName, type: "direct");

            return channel;
        }

        private void StartBasicConsume(string eventName)
        {
            if (this._consumerChannel != null)
            {
                var consumer = new EventingBasicConsumer(this._consumerChannel);

                consumer.Received += this.Consumer_Received;

                this._consumerChannel.BasicConsume(queue: base.GetSubName(eventName), autoAck: false, consumer: consumer);
            }
        }

        private async void Consumer_Received(object? sender, BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey;
            _ = base.ProcessEventName(eventName);
            var message = Encoding.UTF8.GetString(e.Body.Span);

            try
            {
                await base.ProcessEvent(eventName, message);
            }
            catch (Exception ex)
            {
            }

            this._consumerChannel.BasicAck(e.DeliveryTag, multiple: false);
        }
    }
}