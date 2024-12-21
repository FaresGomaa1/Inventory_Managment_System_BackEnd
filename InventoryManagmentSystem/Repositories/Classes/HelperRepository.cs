using InventoryManagmentSystem.Data;
using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Models;
using InventoryManagmentSystem.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InventoryManagmentSystem.Repositories.Classes
{
    public class HelperRepository: IHelperRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly InventoryManagmentContext _context;
        public HelperRepository(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, InventoryManagmentContext context) 
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }   
        public async Task<string> GenerateJwtToken(User user)
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
        public async Task<bool> CheckEmailAndUserName(string email, string username)
        {
            // Ensure either email or username is provided
            return !string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(username);
        }
        public async Task AssignRolesToUser(string role, User user)
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
        public async Task EnsureRoleExistsAndAssign(string roleName, User user)
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
        public async Task ValidateUserDetails(RegisterDTO model)
        {
            var userByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userByEmail != null)
                throw new InvalidOperationException("Email");

            var userByName = await _userManager.FindByNameAsync(model.UserName);
            if (userByName != null)
                throw new InvalidOperationException("UserName");
        }
        public async Task<string?> AssignManager(RegisterDTO model, int userTeamId)
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
        public int AssignTeamId(string role)
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
