using AOP.Aspects;
using DataAccess.DTO;
using DataAccess.Repositories.abstracts;
using Services.Services.abstracts;
using EventHandler.Events.CustomerSupportEvents;
using EventHandler.Handlers.abstracts;

namespace Services.Services.concretes
{
    public class CustomerSupportService : ICustomerSupportService
    {
        private readonly ICustomerSupportRepository _customerSupportRepository;
        private readonly IEventPublisher _eventPublisher;

        public CustomerSupportService(ICustomerSupportRepository customerSupportRepository, IEventPublisher eventPublisher)
        {
            _customerSupportRepository = customerSupportRepository;
            _eventPublisher = eventPublisher;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<CustomerSupportTicketResponseDto> CreateTicketAsync(CustomerSupportTicketCreateDto ticketDto)
        {
            var result = await _customerSupportRepository.CreateAsync(ticketDto);
            
            await _eventPublisher.PublishAsync(new CustomerSupportTicketCreatedEvent
            {
                TicketId = result.Id,
                UserId = result.UserId,
                Issue = result.Issue
            }, "customer_support_events", "ticket_created");

            return result;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        [AuthorizationAspect]
        public async Task<CustomerSupportTicketResponseDto> ResolveTicketAsync(int ticketId, string resolution)
        {
            var updateDto = new CustomerSupportTicketUpdateDto
            {
                Status = "Resolved",
                Resolution = resolution
            };

            var result = await _customerSupportRepository.UpdateAsync(ticketId, updateDto);
            
            if (result != null)
            {
                await _eventPublisher.PublishAsync(new CustomerSupportTicketResolvedEvent
                {
                    TicketId = result.Id,
                    Resolution = result.Resolution
                }, "customer_support_events", "ticket_resolved");
            }

            return result;
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<CustomerSupportTicketResponseDto> GetTicketByIdAsync(int ticketId)
        {
            return await _customerSupportRepository.GetByIdAsync(ticketId);
        }

        [LoggingAspect]
        [ExceptionAspect]
        [PerformanceAspect]
        public async Task<IEnumerable<CustomerSupportTicketResponseDto>> GetAllTicketsAsync()
        {
            return await _customerSupportRepository.GetAllAsync();
        }
    }
}