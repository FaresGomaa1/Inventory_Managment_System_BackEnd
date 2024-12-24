using Microsoft.AspNetCore.Mvc;
using InventoryManagmentSystem.Repositories.Interfaces;
using InventoryManagmentSystem.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace InventoryManagmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<ActionResult<ICollection<GetCategoryDTO>>> GetCategories()
        {
            try
            {
                var categories = await _categoryRepository.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving categories.", details = ex.Message });
            }
        }

        // POST: api/Category
        [HttpPost]
        [Authorize(Roles = "Staff Member, Staff Member Manager")]
        public async Task<ActionResult> AddCategory([FromBody] string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return BadRequest(new { message = "Category name cannot be null or empty." });
            }

            try
            {
                await _categoryRepository.AddCatAsync(categoryName);
                return CreatedAtAction(nameof(GetCategories), new { categoryName }, null);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the category.", details = ex.Message });
            }
        }
    }
}
