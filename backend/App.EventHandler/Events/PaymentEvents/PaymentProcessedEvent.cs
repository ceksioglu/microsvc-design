using EventHandler.Handlers.abstracts;

namespace EventHandler.Events.PaymentEvents;

public class PaymentProcessedEvent : BaseEvent
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public bool IsSuccessful { get; set; }
    public string TransactionId { get; set; }
    public DateTime ProcessedAt { get; set; }
    public Guid Id { get; }
    public DateTime Timestamp { get; }
}