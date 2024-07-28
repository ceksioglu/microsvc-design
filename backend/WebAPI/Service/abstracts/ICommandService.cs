using WebAPI.Models;

namespace WebAPI.Service.abstracts
{
    /// <summary>
    /// Defines the contract for command operations in the e-commerce system.
    /// </summary>
    public interface ICommandService
    {
        Task<Order> CreateOrderAsync(Order order);
        Task<Feedback> SubmitFeedbackAsync(Feedback feedback);
    }
}