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
    internal class OrderPaymentFailedIntegrationHandler : IIntegrationEventHandler<OrderPaymentFailedIntegrationEvent>
    {
        private readonly ILogger<OrderStartedIntegrationEventHandler> _logger;
        public OrderPaymentFailedIntegrationHandler(ILogger<OrderStartedIntegrationEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(OrderPaymentFailedIntegrationEvent @event)
        {
            //send fail email

            _logger.LogInformation($"Order Payment failed with OrderId: {@event.OrderId}, ErroMessage: {@event.ErrorMessage}");
            return Task.CompletedTask;
        }
    }
}
