namespace EventHandler.Events.CustomerSupportEvents;

public class CustomerSupportTicketCreatedEvent : BaseEvent
{
    public int TicketId { get; set; }
    public int UserId { get; set; }
    public string Issue { get; set; }
}