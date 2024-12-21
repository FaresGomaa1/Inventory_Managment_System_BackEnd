using InventoryManagmentSystem.Data;
using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Models;
using InventoryManagmentSystem.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace InventoryManagmentSystem.Repositories.Classes
{
    public class RequestRepository : IRequestRepository
    {
        private readonly InventoryManagmentContext _context;
        private readonly UserManager<User> _userManager;

        // Inject dependencies into the repository
        public RequestRepository(InventoryManagmentContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task CreateRequest(AddRequest requestDetails)
        {
            if (requestDetails.RequestType != "Update Request" && requestDetails.RequestType != "Add Request")
            {
                throw new InvalidOperationException("Invalid RequestType. This API only supports 'Update Request' or 'Add Request'.");
            }
            switch (requestDetails.RequestType)
            {
                case "Add Request":
                    await HandleAddRequestAsync(requestDetails);
                    break;

                case "Update Request":
                    await CheckSKU(requestDetails.SKU);
                    break;

                default:
                    throw new ArgumentException($"Unsupported request type: {requestDetails.RequestType}", nameof(requestDetails.RequestType));
            }
            // Fetch the user details
            User user = await GetUserByIdAsync(requestDetails.UserId);

            // Create a new request based on the provided details
            var newRequest = new Request
            {
                Name = requestDetails.ProductName,
                Price = requestDetails.Price,
                SKU = requestDetails.SKU,
                Quantity = requestDetails.Quantity,
                Description = requestDetails.Description,
                CreatedOn = DateTime.UtcNow,
                CategoryId = requestDetails.CategoryId,
                SupplierId = requestDetails.SupplierId,
                UserId = requestDetails.UserId,
                TeamId = user.TeamId,
                RequestType = requestDetails.RequestType,
                RquestStatus = "New Request",
                Status = true,
            };

            // Save the new request to the database
            await _context.Requests.AddAsync(newRequest);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRequest(UpdateRequest updateRequest)
        {
            // Fetch the user details
            User user = await GetUserByIdAsync(updateRequest.UserId);
            // Fetch the request to be updated
            var request = await _context.Requests.FirstOrDefaultAsync(r => r.Id == updateRequest.RequestId);
            if (request == null)
            {
                throw new InvalidOperationException("Request not found");
            }
            await CheckSKU(updateRequest.SKU);
            // Update request properties
            request.Description = updateRequest.Description;
            request.Name = updateRequest.ProductName;
            request.Price = updateRequest.Price;
            request.Quantity = updateRequest.Quantity;
            request.CategoryId = updateRequest.CategoryId;
            request.SupplierId = updateRequest.SupplierId;
            request.UserId = updateRequest.UserId;
            request.TeamId = user.TeamId;
            request.RequestType = updateRequest.RequestType;
            request.RquestStatus = updateRequest.RquestStatus;

            // Save changes to the database
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductRequest(string SKU, string userId)
        {
            // Fetch the existing product
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.SKU == SKU);
            if (existingProduct == null)
            {
                throw new InvalidOperationException($"Product with SKU {SKU} not found.");
            }

            // Fetch the user details
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found.");
            }

            // Create a new request object based on the existing product
            var newRequest = new Request
            {
                Name = existingProduct.Name,
                Price = existingProduct.Price,
                Quantity = existingProduct.Quantity,
                SKU = existingProduct.SKU,
                CategoryId = existingProduct.CategoryId,
                SupplierId = existingProduct.SupplierId,
                UserId = userId,
                Description = existingProduct.Description,
                TeamId = user.TeamId,
                RequestType = "Delete Request",
                CreatedOn = DateTime.UtcNow,
                Status = true,
                RquestStatus = "New Request"

            };

            // Add and save the new request
            await _context.Requests.AddAsync(newRequest);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> HasOnlyOneActiveRequestForProductAsync(string sku)
        {
            int activeRequestCount = await _context.Requests
                .Where(r => r.SKU == sku && r.Status == true)
                .CountAsync();
            return activeRequestCount == 1;
        }

        // Utility method to fetch a user by ID
        private async Task<User> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found.");
            }
            return user;
        }
        private async Task CheckSKU(string newSKU)
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(r => r.SKU == newSKU);
            if (existingProduct != null && newSKU != existingProduct.SKU)
            {
                throw new InvalidOperationException($"SKU '{newSKU}' already exists in the system.");
            }
        }
        private async Task HandleAddRequestAsync(AddRequest requestDetails)
        {
            // Check if the SKU already exists
            bool skuExists = await _context.Products.AnyAsync(r => r.SKU == requestDetails.SKU);
            if (skuExists)
            {
                // Log and throw exception if SKU already exists
                throw new InvalidOperationException("SKU already exists in the system.");
            }
        }
    }
}