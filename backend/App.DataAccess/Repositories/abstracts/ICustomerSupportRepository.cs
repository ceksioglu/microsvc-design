using DataAccess.DTO;
using DataAccess.Models;

namespace DataAccess.Repositories.abstracts
{
    public interface ICustomerSupportRepository
    {
        Task<CustomerSupportTicketResponseDto> CreateAsync(CustomerSupportTicketCreateDto ticketDto);
        Task<CustomerSupportTicketResponseDto> GetByIdAsync(int id);
        Task<CustomerSupportTicketResponseDto> UpdateAsync(int id, CustomerSupportTicketUpdateDto ticketDto);
        Task<IEnumerable<CustomerSupportTicketResponseDto>> GetAllAsync();
    }
}