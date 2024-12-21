using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Models;
using InventoryManagmentSystem.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagmentSystem.Repositories.Classes
{
    public class UserRepository: IUserRepository  
    {
        public readonly UserManager<User> _userManager;
        public readonly RoleManager<IdentityRole> _roleManager;
        public readonly IUserHelperRepository _helperRepository;

        public UserRepository(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IUserHelperRepository helperRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _helperRepository = helperRepository;
        }
        public async Task<(bool IsSuccess, string Message, string Token)> LoginUserAsync(LoginDTO model)
        {
            // Check if email or username is provided
            if (!await _helperRepository.CheckEmailAndUserName(model.Email, model.UserName))
            {
                return (false, "You must provide either an email or a username.", null);
            }

            // Find user by email or username
            User user = null;
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                user = await _userManager.FindByEmailAsync(model.Email);
            }
            else
            {
                user = await _userManager.FindByNameAsync(model.UserName);
            }

            if (user == null)
            {
                return (false, "Invalid username, email, or password.", null);
            }

            // Verify password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                return (false, "Invalid username, email, or password.", null);
            }

            // Generate JWT token
            var token = await _helperRepository.GenerateJwtToken(user);

            return (true, "Login successful.", token);
        }
        public async Task<string> CreateUser(RegisterDTO model)
        {
            // Step 1: Validate Email and Username
            await _helperRepository.ValidateUserDetails(model);

            // Step 2: Create a new User object
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                UserName = model.UserName,
                Email = model.Email,
            };

            // Step 3: Handle Manager Assignment
            if (!model.IsManager && !string.IsNullOrEmpty(model.ManagerId))
            {
                user.TeamId = _helperRepository.AssignTeamId(model.Role);
                user.ManagerId = await _helperRepository.AssignManager(model, user.TeamId);
            }
            else
            {
                // Validate and process the role
                var roleParts = model.Role.Split(' ');

                if (roleParts.Length == 3)
                {
                    var lastWord = roleParts.Last();

                    if (string.Equals(lastWord, "Manager", StringComparison.OrdinalIgnoreCase))
                    {
                        // Step 4: Assign Team Based on Role
                        user.TeamId = _helperRepository.AssignTeamId(model.Role);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid role format: {model.Role}");
                    }
                }
                else
                {
                    throw new ArgumentException($"Role must contain exactly three words: {model.Role}");
                }
            }
            // Step 5: Create User
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // Step 6: Assign Roles to User
            await _helperRepository.AssignRolesToUser(model.Role, user);

            return "User created successfully!";
        }
    }
}
