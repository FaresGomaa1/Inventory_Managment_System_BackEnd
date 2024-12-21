using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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
            if (await _requestRepository.HasOnlyOneActiveRequestForProductAsync(requestDetails.SKU))
            {
                // If there is already one active request, return a conflict response
                return Conflict(new
                {
                    Message = "Conflict: There is already an active request for this product.",
                    Errors = "An active request already exists for the given SKU.",
                    SKU = requestDetails.SKU
                });
            }
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

        [HttpPost("DeleteRequest")]
        [Authorize(Roles = "Staff Member, Staff Member Manager")]
        public async Task<IActionResult> DeleteRequest(string SKU,string userId)
        {
            if (string.IsNullOrEmpty(SKU))
            {
                return BadRequest(new
                {
                    Message = "SKU is required.",
                    Errors = "The SKU field cannot be null or empty.",
                    Timestamp = DateTime.UtcNow
                });
            }
            if (await _requestRepository.HasOnlyOneActiveRequestForProductAsync(SKU))
            {
                // If there is already one active request, return a conflict response
                return Conflict(new
                {
                    Message = "Conflict: There is already an active request for this product.",
                    Errors = "An active request already exists for the given SKU.",
                    SKU = SKU
                });
            }
            try
            {
                await _requestRepository.DeleteProductRequest(SKU, userId);

                // Return a success response with the newly created request details
                return Ok(new
                {
                    Message = "Delete request created successfully.",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                // Handle specific errors such as product not found
                return NotFound(new
                {
                    Message = "The product was not found or could not be processed.",
                    Errors = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                // Log and return a generic error response
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An unexpected error occurred while processing the request.",
                    Errors = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPut("UpdateRequest")]
        [Authorize(Roles = "Staff Member, Staff Member Manager")]
        public async Task<IActionResult> UpdateRequest([FromBody] UpdateRequest updateRequest)
        {
            if (updateRequest == null)
                return BadRequest(new
                {
                    Message = "Invalid input data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors)
                              .Select(e => e.ErrorMessage)
                });
            if (await _requestRepository.HasOnlyOneActiveRequestForProductAsync(updateRequest.SKU))
            {
                // If there is already one active request, return a conflict response
                return Conflict(new
                {
                    Message = "Conflict: There is already an active request for this product.",
                    Errors = "An active request already exists for the given SKU.",
                    SKU = updateRequest.SKU
                });
            }
            try
            {
                // Call the repository to update the request
                await _requestRepository.UpdateRequest(updateRequest);

                // Return a 200 OK response with a success message
                return Ok(new
                {
                    Message = "Request updated successfully",
                    RequestId = updateRequest.RequestId
                });
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("SKU", StringComparison.OrdinalIgnoreCase))
                {
                    return Conflict(new
                    {
                        Message = "SKU already exists.",
                        Errors = ex.Message,
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Handle other specific exceptions (e.g., request not found)
                return NotFound(new
                {
                    Message = "Request not found.",
                    Errors = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                // Log and return a generic error response
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
        }
    }
}
