namespace EventHandler.Events.CustomerSupportEvents;

public class CustomerSupportTicketResolvedEvent : BaseEvent
{
    public int TicketId { get; set; }
    public string Resolution { get; set; }
}