using EventHandler.Events.OrderEvents;
using EventHandler.Events.PaymentEvents;
using EventHandler.Handlers.abstracts;
using Microsoft.Extensions.Logging;
using SagaOrchestrator.abstracts;

namespace SagaOrchestrator.concretes
{
    public class SagaOrchestrator : ISagaOrchestrator
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<SagaOrchestrator> _logger;

        public SagaOrchestrator(IEventPublisher eventPublisher, ILogger<SagaOrchestrator> logger)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task ProcessEventAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            _logger.LogInformation($"Processing event: {@event.GetType().Name} with ID: {@event.Id} at {@event.Timestamp}");

            switch (@event)
            {
                case OrderCreatedEvent orderCreatedEvent:
                    await HandleOrderCreatedAsync(orderCreatedEvent);
                    break;
                case PaymentProcessedEvent paymentProcessedEvent:
                    await HandlePaymentProcessedAsync(paymentProcessedEvent);
                    break;
                case InventoryUpdatedEvent inventoryUpdatedEvent:
                    await HandleInventoryUpdatedAsync(inventoryUpdatedEvent);
                    break;
                case OrderShippedEvent orderShippedEvent:
                    await HandleOrderShippedAsync(orderShippedEvent);
                    break;
                default:
                    _logger.LogWarning($"Unhandled event type: {@event.GetType().Name}");
                    break;
            }
        }

        private async Task HandleOrderCreatedAsync(OrderCreatedEvent @event)
        {
            _logger.LogInformation($"Handling OrderCreatedEvent for Order ID: {@event.OrderId}");
            await _eventPublisher.PublishAsync(new ProcessPaymentCommand(@event.OrderId, @event.TotalAmount), "payment_commands", "process_payment");
        }

        private async Task HandlePaymentProcessedAsync(PaymentProcessedEvent @event)
        {
            _logger.LogInformation($"Handling PaymentProcessedEvent for Order ID: {@event.OrderId}");
            if (@event.IsSuccessful)
            {
                await _eventPublisher.PublishAsync(new UpdateInventoryCommand(@event.OrderId), "inventory_commands", "update_inventory");
            }
            else
            {
                await _eventPublisher.PublishAsync(new CancelOrderCommand(@event.OrderId, "Payment failed"), "order_commands", "cancel_order");
            }
        }

        private async Task HandleInventoryUpdatedAsync(InventoryUpdatedEvent @event)
        {
            _logger.LogInformation($"Handling InventoryUpdatedEvent for Order ID: {@event.OrderId}");
            if (@event.Success)
            {
                await _eventPublisher.PublishAsync(new PrepareShipmentCommand(@event.OrderId), "shipping_commands", "prepare_shipment");
            }
            else
            {
                await _eventPublisher.PublishAsync(new CancelOrderCommand(@event.OrderId, "Insufficient inventory"), "order_commands", "cancel_order");
                await _eventPublisher.PublishAsync(new RefundPaymentCommand(@event.OrderId), "payment_commands", "refund_payment");
            }
        }

        private async Task HandleOrderShippedAsync(OrderShippedEvent @event)
        {
            _logger.LogInformation($"Handling OrderShippedEvent for Order ID: {@event.OrderId}");
            await _eventPublisher.PublishAsync(new CompleteOrderCommand(@event.OrderId), "order_commands", "complete_order");
            await _eventPublisher.PublishAsync(new NotifyCustomerCommand(@event.OrderId, $"Your order {@event.OrderId} has been shipped with tracking number {@event.TrackingNumber}!"), "notification_commands", "notify_customer");
        }
    }