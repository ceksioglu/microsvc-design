using AutoMapper;
using DataAccess.DTO;
using DataAccess.Models;
using DataAccess.Repositories.abstracts;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.concretes
{
    public class CustomerSupportRepository : ICustomerSupportRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CustomerSupportRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CustomerSupportTicketResponseDto> CreateAsync(CustomerSupportTicketCreateDto ticketDto)
        {
            var ticket = _mapper.Map<CustomerSupportTicket>(ticketDto);
            ticket.Status = "Open";
            ticket.CreatedAt = DateTime.UtcNow;

            _context.CustomerSupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            return _mapper.Map<CustomerSupportTicketResponseDto>(ticket);
        }

        public async Task<CustomerSupportTicketResponseDto> GetByIdAsync(int id)
        {
            var ticket = await _context.CustomerSupportTickets.FindAsync(id);
            return _mapper.Map<CustomerSupportTicketResponseDto>(ticket);
        }

        public async Task<CustomerSupportTicketResponseDto> UpdateAsync(int id, CustomerSupportTicketUpdateDto ticketDto)
        {
            var ticket = await _context.CustomerSupportTickets.FindAsync(id);
            if (ticket == null)
                return null;

            _mapper.Map(ticketDto, ticket);
            if (ticketDto.Status == "Resolved" && !ticket.ResolvedAt.HasValue)
                ticket.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<CustomerSupportTicketResponseDto>(ticket);
        }

        public async Task<IEnumerable<CustomerSupportTicketResponseDto>> GetAllAsync()
        {
            var tickets = await _context.CustomerSupportTickets.ToListAsync();
            return _mapper.Map<IEnumerable<CustomerSupportTicketResponseDto>>(tickets);
        }
    }
}