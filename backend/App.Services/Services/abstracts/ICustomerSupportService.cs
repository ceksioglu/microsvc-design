using DataAccess.DTO;

namespace Services.Services.abstracts
{
    public interface ICustomerSupportService
    {
        Task<CustomerSupportTicketResponseDto> CreateTicketAsync(CustomerSupportTicketCreateDto ticketDto);
        Task<CustomerSupportTicketResponseDto> ResolveTicketAsync(int ticketId, string resolution);
        Task<CustomerSupportTicketResponseDto> GetTicketByIdAsync(int ticketId);
        Task<IEnumerable<CustomerSupportTicketResponseDto>> GetAllTicketsAsync();
    }
}