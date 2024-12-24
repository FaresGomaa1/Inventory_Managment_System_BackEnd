using Microsoft.AspNetCore.Mvc;
using InventoryManagmentSystem.Repositories.Interfaces;
using InventoryManagmentSystem.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace InventoryManagmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierController(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        // GET: api/Supplier
        [HttpGet]
        public async Task<ActionResult<ICollection<GetSupplierDTO>>> GetSuppliers()
        {
            try
            {
                var suppliers = await _supplierRepository.GetAllSuppliersAsync();
                return Ok(suppliers);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving suppliers.", details = ex.Message });
            }
        }

        // GET: api/Supplier/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GetSupplierDTO>> GetSupplier(int id)
        {
            try
            {
                var supplier = await _supplierRepository.GetSupplierAsync(id);
                return Ok(supplier);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the supplier.", details = ex.Message });
            }
        }

        // POST: api/Supplier
        [HttpPost]
        [Authorize(Roles = "Staff Member, Staff Member Manager")]
        public async Task<ActionResult> AddSupplier([FromBody] AddSupplierDTO supplierDto)
        {
            if (supplierDto == null)
            {
                return BadRequest(new { message = "Supplier data cannot be null." });
            }

            try
            {
                await _supplierRepository.AddSupplierAsync(supplierDto);
                return CreatedAtAction(nameof(GetSupplier), new { id = supplierDto }, null);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the supplier.", details = ex.Message });
            }
        }
    }
}
