using WebAPI.DTO;

namespace WebAPI.Services.abstracts;

    public interface IProductQueryService
    {
        /// <summary>
        /// Retrieves a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the product details.</returns>
        Task<ProductResponseDto> GetProductByIdAsync(int id);

        /// <summary>
        /// Retrieves all products.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all products.</returns>
        Task<IEnumerable<ProductListItemDto>> GetAllProductsAsync();

        /// <summary>
        /// Searches for products based on a given term.
        /// </summary>
        /// <param name="term">The search term.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of products matching the search term.</returns>
        Task<IEnumerable<ProductListItemDto>> SearchProductsAsync(string term);

        /// <summary>
        /// Retrieves products by category.
        /// </summary>
        /// <param name="category">The category to filter by.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of products in the specified category.</returns>
        Task<IEnumerable<ProductListItemDto>> GetProductsByCategoryAsync(string category);

        /// <summary>
        /// Checks if a product is in stock for a given quantity.
        /// </summary>
        /// <param name="id">The ID of the product to check.</param>
        /// <param name="quantity">The quantity to check for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating if the product is in stock.</returns>
        Task<bool> IsInStockAsync(int id, int quantity);

        /// <summary>
        /// Retrieves featured products.
        /// </summary>
        /// <param name="count">The number of featured products to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of featured products.</returns>
        Task<IEnumerable<ProductListItemDto>> GetFeaturedProductsAsync(int count);

        /// <summary>
        /// Retrieves products related to a given product.
        /// </summary>
        /// <param name="productId">The ID of the product to find related products for.</param>
        /// <param name="count">The number of related products to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of related products.</returns>
        Task<IEnumerable<ProductListItemDto>> GetRelatedProductsAsync(int productId, int count);
    }