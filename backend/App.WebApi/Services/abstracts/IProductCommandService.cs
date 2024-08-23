using WebAPI.DTO;

namespace WebAPI.Services.abstracts;

public interface IProductCommandService
{
    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="productDto">The details of the product to create.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the details of the created product.</returns>
    Task<ProductResponseDto> CreateProductAsync(ProductCreateDto productDto);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The ID of the product to update.</param>
    /// <param name="productDto">The updated details of the product.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated product details.</returns>
    Task<ProductResponseDto> UpdateProductAsync(int id, ProductUpdateDto productDto);

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="id">The ID of the product to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating if the deletion was successful.</returns>
    Task<bool> DeleteProductAsync(int id);

    /// <summary>
    /// Updates the stock quantity of a product.
    /// </summary>
    /// <param name="id">The ID of the product to update.</param>
    /// <param name="quantity">The new stock quantity.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating if the update was successful.</returns>
    Task<bool> UpdateStockQuantityAsync(int id, int quantity);
}