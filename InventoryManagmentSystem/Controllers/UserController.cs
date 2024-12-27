using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using InventoryManagmentSystem.Models;
using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Repositories.Interfaces;

namespace InventoryManagmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public readonly UserManager<User> _userManager;
        public readonly RoleManager<IdentityRole> _roleManager;
        public readonly IUserRepository _userRepository;

        public UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IUserRepository userRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userRepository = userRepository;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Message = "Invalid input data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                });
            }

            try
            {
                // Attempt to create user
                string status = await _userRepository.CreateUser(model);

                // You can customize the status further depending on your CreateUser method's response
                return Ok(new
                {
                    Message = "User registered successfully",
                    Status = status
                });
            }
            catch (InvalidOperationException ex)
            {
                switch (ex.Message)
                {
                    case "Email":
                        return Conflict(new { Message = "The email address is already taken." });

                    case "UserName":
                        return Conflict(new { Message = "The User Name is already taken." });

                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new
                        {
                            Message = "An unexpected error occurred during user registration.",
                            Details = ex.Message 
                        });
                }
            }

            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    Message = "Invalid role specified",
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                // Log the error details (optional: use a logging framework like Serilog or NLog)
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while creating the user.",
                    Details = ex.Message
                });
            }
        }




        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Message = "Invalid input data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            // Delegate login logic to the repository
            var loginResult = await _userRepository.LoginUserAsync(model);

            if (!loginResult.IsSuccess)
            {
                return Unauthorized(new { Message = loginResult.Message });
            }

            return Ok(new
            {
                Message = "Login successful.",
                Token = loginResult.Token
            });
        }
        [HttpGet("Managers")]
        public async Task<IActionResult> GetAllManagers([FromQuery] string managerTeam)
        {
            try
            {
                var managers = await _userRepository.GetAllManagersAsync(managerTeam);

                if (managers == null || !managers.Any())
                {
                    return NotFound("No managers found.");
                }

                return Ok(managers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {message = ex.Message});
            }
        }


    }
}