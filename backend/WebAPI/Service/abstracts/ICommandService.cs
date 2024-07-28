using WebAPI.Models;

namespace WebAPI.Service.abstracts
{
    /// <summary>
    /// Defines the contract for command operations in the e-commerce system.
    /// </summary>
    public interface ICommandService
    {
        /// <summary>
        /// Creates a new order in the system.
        /// </summary>
        /// <param name="request">The order details.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
        Task<bool> CreateOrderAsync(CreateOrderRequest request);

        /// <summary>
        /// Submits customer feedback.
        /// </summary>
        /// <param name="request">The feedback details.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
        Task<bool> SubmitFeedbackAsync(SubmitFeedbackRequest request);
    }
}