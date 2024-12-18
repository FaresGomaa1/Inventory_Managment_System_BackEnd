using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace InventoryManagmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IRequestRepository _requestRepository;

        // Inject the IRequestRepository into the controller
        public RequestController(IRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }
        [HttpPost("CreateRequest")]
        [Authorize(Roles = "Staff Member, Staff Member Manager")]
        public async Task<IActionResult> CreateRequest([FromBody] AddRequest requestDetails)
        {
            if (requestDetails == null)
                return BadRequest(new
                {
                    Message = "Invalid input data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors)
                              .Select(e => e.ErrorMessage)
                });

            try
            {
                // Call the repository to create the request
                await _requestRepository.CreateRequest(requestDetails);

                // Return a 201 Created response with a success message
                return CreatedAtAction(nameof(CreateRequest), new { SKU = requestDetails.SKU }, requestDetails);
            }
            catch (InvalidOperationException ex)
            {
                // If the SKU already exists, return a conflict status code
                return Conflict(new
                {
                    Message = "Conflict: The data already exists.",
                    Errors = ex.Message,
                    Timestamp = DateTime.UtcNow,
                    Resolution = "Please ensure the data is unique or check for existing records before attempting to create new ones.", // Suggest resolution steps
                    Details = new
                    {
                        SKU = requestDetails?.SKU
                    }
                });
            }
            catch (Exception ex)
            {
                // Log and return a generic error response
                // You can log the error here if necessary
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
        }
    }
}
