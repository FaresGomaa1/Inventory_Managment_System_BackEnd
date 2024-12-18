using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Models;
using InventoryManagmentSystem.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InventoryManagmentSystem.Repositories.Classes
{
    public class UserRepository: IUserRepository  
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<(bool IsSuccess, string Message, string Token)> LoginUserAsync(LoginDTO model)
        {
            // Check if email or username is provided
            if (!await CheckEmailAndUserName(model.Email, model.UserName))
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
            var token = await GenerateJwtToken(user);

            return (true, "Login successful.", token);
        }
        public async Task<string> CreateUser(RegisterDTO model)
        {
            // Step 1: Validate Email and Username
            await ValidateUserDetails(model);

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
                user.TeamId = AssignTeamId(model.Role);
                user.ManagerId = await AssignManager(model, user.TeamId);
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
                        user.TeamId = AssignTeamId(model.Role);
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
            await AssignRolesToUser(model.Role, user);

            return "User created successfully!";
        }
        private async Task<string> GenerateJwtToken(User user)
        {
            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Create claims
            var claims = new List<Claim>
                                {
                                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                                };

            // Add role claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Define signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourLongerSuperSecretKeyHere123456"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create JWT token
            var token = new JwtSecurityToken(
                issuer: "YourIssuer",
                audience: "YourAudience",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private async Task<bool> CheckEmailAndUserName(string email, string username)
        {
            // Ensure either email or username is provided
            return !string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(username);
        }
        private async Task AssignRolesToUser(string role, User user)
        {
            switch (role)
            {
                case "Staff Member":
                    await EnsureRoleExistsAndAssign("Staff Member", user);
                    break;
                case "Staff Member Manager":
                    await EnsureRoleExistsAndAssign("Staff Member Manager", user);
                    break;
                case "Inventory Manager":
                    await EnsureRoleExistsAndAssign("Inventory Manager", user);
                    break;
                case "Inventory Manager Manager":
                    await EnsureRoleExistsAndAssign("Inventory Manager Manager", user);
                    break;
                case "Department Manager":
                    await EnsureRoleExistsAndAssign("Department Manager", user);
                    break;
                case "Department Manager Manager":
                    await EnsureRoleExistsAndAssign("Department Manager Manager", user);
                    break;
                default:
                    throw new ArgumentException("Invalid role specified");
            }
        }
        private async Task EnsureRoleExistsAndAssign(string roleName, User user)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!addToRoleResult.Succeeded)
            {
                throw new Exception($"Failed to assign role '{roleName}' to user: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
            }
        }
        private async Task ValidateUserDetails(RegisterDTO model)
        {
            var userByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userByEmail != null)
                throw new InvalidOperationException("Email");

            var userByName = await _userManager.FindByNameAsync(model.UserName);
            if (userByName != null)
                throw new InvalidOperationException("UserName");
        }
        private async Task<string?> AssignManager(RegisterDTO model, int userTeamId)
        {
            var manager = await _userManager.FindByIdAsync(model.ManagerId);
            if (manager == null)
            {
                throw new ArgumentException($"Manager with ID {model.ManagerId} not found.");
            }

            var roles = await _userManager.GetRolesAsync(manager);
            if (roles == null || roles.Count == 0)
            {
                throw new InvalidOperationException($"Manager with ID {model.ManagerId} has no assigned roles.");
            }

            if (manager.TeamId != userTeamId)
            {
                throw new InvalidOperationException($"Manager's role does not match the expected role for {model.Role}.");
            }

            return model.ManagerId;
        }
        private int AssignTeamId(string role)
        {
            return role switch
            {
                "Staff Member" or "Staff Member Manager" => 1,
                "Inventory Manager" or "Inventory Manager Manager" => 2,
                "Department Manager" or "Department Manager Manager" => 3,
                _ => throw new ArgumentException("Invalid role specified")
            };
        }
        
    }
}
