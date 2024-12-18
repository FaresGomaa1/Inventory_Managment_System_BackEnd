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
        // Create a user with specified roles
        public async Task<string> CreateUser(RegisterDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
                throw new InvalidOperationException("Email");
            user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null)
                throw new InvalidOperationException("UserName");

            user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                UserName = model.UserName,
                Email = model.Email,
            };
            switch (model.Role)
            {
                case "Staff Member":
                    user.TeamId = 1;
                    break;
                case "Inventory Manager":
                    user.TeamId = 2;
                    break;
                case "Department Manager":
                    user.TeamId = 3;
                    break;
                default:
                    throw new ArgumentException("Invalid role specified");
            }
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await AssignRolesToUser(model.Role, user);
                return "User created successfully!";
            }
            else
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        private async Task<bool> CheckEmailAndUserName(string email, string username)
        {
            // Ensure either email or username is provided
            return !string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(username);
        }
        // Assign roles to the user based on the provided role
        private async Task AssignRolesToUser(string role, User user)
        {
            switch (role)
            {
                case "Staff Member":
                    await EnsureRoleExistsAndAssign("Request-C", user);
                    await EnsureRoleExistsAndAssign("Request-R", user);
                    await EnsureRoleExistsAndAssign("Request-U", user);
                    await EnsureRoleExistsAndAssign("Request-D", user);
                    await EnsureRoleExistsAndAssign("Product-R", user);
                    await EnsureRoleExistsAndAssign("Category-C", user);
                    await EnsureRoleExistsAndAssign("Category-U", user);
                    await EnsureRoleExistsAndAssign("Category-R", user);
                    await EnsureRoleExistsAndAssign("Supplier-R", user);
                    await EnsureRoleExistsAndAssign("Supplier-C", user);
                    await EnsureRoleExistsAndAssign("Supplier-U", user);
                    break;

                case "Inventory Manager":
                case "Department Manager":
                    await EnsureRoleExistsAndAssign("Request-R", user);
                    await EnsureRoleExistsAndAssign("Request-U", user);
                    await EnsureRoleExistsAndAssign("Product-R", user);
                    await EnsureRoleExistsAndAssign("Category-R", user);
                    await EnsureRoleExistsAndAssign("Supplier-R", user);
                    break;

                default:
                    throw new ArgumentException("Invalid role specified");
            }
        }
        // Helper method to ensure role exists and assign it to the user
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

    }
}
