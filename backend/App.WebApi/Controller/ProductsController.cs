using AOP.Aspects;
using DataAccess.DTO;
using Microsoft.AspNetCore.Mvc;
using Services.Services.abstracts;

namespace WebAPI.Controller
{
    /// <summary>
    /// Manages product-related operations such as creation, retrieval, update, and deletion.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductCommandService _productCommandService;
        private readonly IProductQueryService _productQueryService;

        public ProductsController(IProductCommandService productCommandService, IProductQueryService productQueryService)
        {
            _productCommandService = productCommandService;
            _productQueryService = productQueryService;
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="productDto">The product information to create.</param>
        /// <returns>The created product's information.</returns>
        /// <response code="201">Returns the newly created product.</response>
        /// <response code="400">If the product data is invalid.</response>
        [HttpPost]
        [AuthorizationAspect("Admin")]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto productDto)
        {
            var result = await _productCommandService.CreateProductAsync(productDto);
            return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result);
        }

        /// <summary>
        /// Retrieves a specific product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to retrieve.</param>
        /// <returns>The product's information.</returns>
        /// <response code="200">Returns the requested product.</response>
        /// <response code="404">If the product is not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productQueryService.GetProductByIdAsync(id);
            return product != null ? Ok(product) : NotFound();
        }

        /// <summary>
        /// Retrieves all products.
        /// </summary>
        /// <returns>A list of all products.</returns>
        /// <response code="200">Returns the list of all products.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productQueryService.GetAllProductsAsync();
            return Ok(products);
        }

        /// <summary>
        /// Updates a product's information.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="productDto">The updated product information.</param>
        /// <returns>The updated product's information.</returns>
        /// <response code="200">Returns the updated product.</response>
        /// <response code="400">If the update data is invalid.</response>
        /// <response code="404">If the product is not found.</response>
        [HttpPut("{id}")]
        [AuthorizationAspect("Admin")]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDto productDto)
        {
            var updatedProduct = await _productCommandService.UpdateProductAsync(id, productDto);
            return updatedProduct != null ? Ok(updatedProduct) : NotFound();
        }

        /// <summary>
        /// Deletes a product.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the product was successfully deleted.</response>
        /// <response code="404">If the product is not found.</response>
        [HttpDelete("{id}")]
        [AuthorizationAspect("Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productCommandService.DeleteProductAsync(id);
            return result ? NoContent() : NotFound();
        }

        /// <summary>
        /// Searches for products based on a given term.
        /// </summary>
        /// <param name="term">The search term.</param>
        /// <returns>A list of products matching the search term.</returns>
        /// <response code="200">Returns the list of products matching the search term.</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<ProductListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchProducts([FromQuery] string term)
        {
            var products = await _productQueryService.SearchProductsAsync(term);
            return Ok(products);
        }

        /// <summary>
        /// Retrieves products by category.
        /// </summary>
        /// <param name="category">The category to filter by.</param>
        /// <returns>A list of products in the specified category.</returns>
        /// <response code="200">Returns the list of products in the specified category.</response>
        [HttpGet("category/{category}")]
        [ProducesResponseType(typeof(IEnumerable<ProductListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductsByCategory(string category)
        {
            var products = await _productQueryService.GetProductsByCategoryAsync(category);
            return Ok(products);
        }

        /// <summary>
        /// Retrieves featured products.
        /// </summary>
        /// <param name="count">The number of featured products to retrieve.</param>
        /// <returns>A list of featured products.</returns>
        /// <response code="200">Returns the list of featured products.</response>
        [HttpGet("featured")]
        [ProducesResponseType(typeof(IEnumerable<ProductListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeaturedProducts([FromQuery] int count = 5)
        {
            var products = await _productQueryService.GetFeaturedProductsAsync(count);
            return Ok(products);
        }

        /// <summary>
        /// Retrieves products related to a given product.
        /// </summary>
        /// <param name="productId">The ID of the product to find related products for.</param>
        /// <param name="count">The number of related products to retrieve.</param>
        /// <returns>A list of related products.</returns>
        /// <response code="200">Returns the list of related products.</response>
        /// <response code="404">If the product is not found.</response>
        [HttpGet("{productId}/related")]
        [ProducesResponseType(typeof(IEnumerable<ProductListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRelatedProducts(int productId, [FromQuery] int count = 5)
        {
            var products = await _productQueryService.GetRelatedProductsAsync(productId, count);
            return products != null ? Ok(products) : NotFound();
        }

        /// <summary>
        /// Updates the stock quantity of a product.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="quantity">The new stock quantity.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the stock quantity was successfully updated.</response>
        /// <response code="404">If the product is not found.</response>
        [HttpPatch("{id}/stock")]
        [AuthorizationAspect("Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStockQuantity(int id, [FromBody] int quantity)
        {
            var result = await _productCommandService.UpdateStockQuantityAsync(id, quantity);
            return result ? NoContent() : NotFound();
        }
    }
}