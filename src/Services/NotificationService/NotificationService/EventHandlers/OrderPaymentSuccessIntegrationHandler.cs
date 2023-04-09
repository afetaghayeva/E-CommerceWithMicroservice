using EventBus.Base.Abstraction;
using Microsoft.Extensions.Logging;
using PaymentService.API.IntegrationEvents.EventHandlers;
using PaymentService.API.IntegrationEvents.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.EventHandlers
{
    internal class OrderPaymentSuccessIntegrationHandler : IIntegrationEventHandler<OrderPaymentSuccessIntegrationEvent>
    {
        private readonly ILogger<OrderStartedIntegrationEventHandler> _logger;
        public OrderPaymentSuccessIntegrationHandler(ILogger<OrderStartedIntegrationEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(OrderPaymentSuccessIntegrationEvent @event)
        {
            //send fail email

            _logger.LogInformation($"Order Payment success with OrderId: {@event.OrderId}");
            return Task.CompletedTask;
        }
    }
}
