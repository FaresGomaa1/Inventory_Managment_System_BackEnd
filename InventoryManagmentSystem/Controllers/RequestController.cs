using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        public readonly IRequestRepository _requestRepository;

        // Inject the IRequestRepository into the controller
        public RequestController(IRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }

        [HttpPost("CreateRequest")]
        [Authorize(Roles = "Staff Member, Staff Member Manager")]
        public async Task<IActionResult> CreateRequest([FromBody] AddRequest requestDetails)
        {
            // Ensure model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid model data.", Errors = ModelState });
            }
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
        [HttpGet("GetRequests")]
        [Authorize(Roles = "Staff Member, Staff Member Manager, Inventory Manager, Inventory Manager Manager, Department Manager, Department Manager Manager")]
        public IActionResult GetRequests(
            [FromQuery] string viewName,
            [FromQuery] string sortBy,
            [FromQuery] string? userId,
            [FromQuery] bool isAscending)
        {
            try
            {
                // Call the repository to get the requests
                var requests =  _requestRepository.GetRequests(viewName, sortBy, userId, isAscending);

                if (requests == null)
                {
                    return NotFound(new
                    {
                        Message = "No requests found.",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Return the list of requests
                return Ok(new
                {
                    Message = "Requests retrieved successfully.",
                    Data = requests,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                // Log and return a generic error response
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while processing the request.",
                    Errors = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("team/{managerId}")]
        [Authorize(Roles = "Staff Member Manager, Inventory Manager Manager, Department Manager Manager")]
        public IActionResult GetActiveRequestsForEachTeamMember([FromQuery] string managerId)
        {
            try
            {
                var activeRequests = _requestRepository.GetUsersWithActiveRequestsCount(managerId);

                if (activeRequests == null || activeRequests.Count == 0)
                {
                    return NotFound(new
                    {
                        Message = "No active requests found for the specified manager.",
                        Timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new
                {
                    Message = "Active requests retrieved successfully.",
                    Data = activeRequests,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    Message = "An error occurred while processing your request.",
                    ErrorDetails = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPatch("update-manager-decision")]
        [Authorize(Roles = "Inventory Manager, Inventory Manager Manager, Department Manager, Department Manager Manager")]
        public async Task<IActionResult> UpdateManagerDecisionAsync([FromBody] ManagerDecision managerDecision)
        {
            // Ensure model state is valid
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "Invalid model data.", Errors = ModelState });

            // Validate the manager's decision data
            if (managerDecision == null)
                return BadRequest(new { Message = "Decision is required." });

            try
            {
                // Handle the manager's decision asynchronously
                await _requestRepository.HandleManagerDecisionAsync(managerDecision);

                return Ok(new
                {
                    Message = "Manager decision updated successfully.",
                    RequestId = managerDecision.RequestId,
                    Decision = managerDecision.Decision,
                    Comment = managerDecision.Comment
                });
            }
            catch (KeyNotFoundException ex)
            {
                // Handle not found exception (e.g., request not found)
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle unauthorized access exception
                return Unauthorized(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Handle invalid operation (e.g., invalid status)
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Catch any other general exceptions
                return StatusCode(500, new { Message = "An error occurred while processing the request.", Error = ex.Message });
            }
        }
        [HttpGet("GenerateSKU")]
        [Authorize(Roles = "Staff Member, Staff Member Manager")]
        public async Task<IActionResult> GenerateSKU(string sku, string requestType)
        {
            if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(requestType))
            {
                return BadRequest(new
                {
                    message = "SKU and Request Type cannot be null or empty."
                });
            }

            try
            {
                string result = await _requestRepository.GenerateSKU(sku, requestType);
                if (result == null)
                {
                    return NotFound("SKU generation failed or no data found.");
                }

                return Ok(new {message = result });
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while generating the SKU.");
            }
        }
        [HttpGet("GetById")]
        public async Task<IActionResult> GetRequestByIdAsync(int id)
        {
            try
            {
                var result = await _requestRepository.GetRequestById(id);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
            }
        }
    }
}
