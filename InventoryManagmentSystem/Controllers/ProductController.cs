using Microsoft.AspNetCore.Mvc;
using InventoryManagmentSystem.Repositories.Interfaces;
using InventoryManagmentSystem.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace InventoryManagmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<ICollection<GetProductDTO>>> GetProducts()
        {
            try
            {
                var products = await _productRepository.GetAllProductsAsync();
                return Ok(products);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving products.", details = ex.Message });
            }
        }

        // GET: api/Product/{id}
        [HttpGet("{sku}")]
        public async Task<ActionResult<GetProductDTO>> GetProduct(string sku)
        {
            try
            {
                var product = await _productRepository.GetProductAsync(sku);
                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the product.", details = ex.Message });
            }
        }

        // POST: api/Product
        [HttpPost]
        [Authorize(Roles = "Department Manager, Department Manager Manager")]
        public async Task<ActionResult> AddProduct([FromBody] AddProductDTO addProductDTO)
        {
            if (addProductDTO == null)
            {
                return BadRequest(new { message = "Product data cannot be null." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid product data.", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                await _productRepository.AddProductAsync(addProductDTO);
                return CreatedAtAction(nameof(GetProduct), new { id = addProductDTO }, null);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the product.", details = ex.Message });
            }
        }

        // PUT: api/Product
        [HttpPut]
        [Authorize(Roles = "Department Manager, Department Manager Manager")]
        public async Task<ActionResult> UpdateProduct([FromBody] UpdateProductDTO updateProductDTO)
        {
            if (updateProductDTO == null)
            {
                return BadRequest(new { message = "Product data cannot be null." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid product data.", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                await _productRepository.UpdateProductAsync(updateProductDTO);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the product.", details = ex.Message });
            }
        }

        // DELETE: api/Product/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productRepository.DeleteProductAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the product.", details = ex.Message });
            }
        }
    }
}
