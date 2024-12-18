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

        // Inject the DbContext into the repository
        public RequestRepository(InventoryManagmentContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task CreateRequest(AddRequest requestDetails)
        {
            Request existingRequest = await _context.Requests.FirstOrDefaultAsync(r => r.SKU == requestDetails.SKU);
            if (existingRequest != null)
                throw new InvalidOperationException("SKU already exists in the system");

            // Create a new request based on the provided details
            Request newRequest = new Request()
            {
                Name = requestDetails.ProductName,
                Price = requestDetails.Price,
                SKU = requestDetails.SKU,
                Quantity = requestDetails.Quantity,
                Description = requestDetails.Description,
                CreatedOn = DateTime.UtcNow,
                CategoryId = requestDetails.CategoryId,
                SupplierId = requestDetails.SupplierId,
                UserId = requestDetails.UserId
            };

            // Fetch the user details from UserManager
            User user = await _userManager.FindByIdAsync(requestDetails.UserId);
            if (user != null)
            {
                newRequest.TeamId = user.TeamId;
            }

            // Save the new request to the database
            await _context.Requests.AddAsync(newRequest);
            await _context.SaveChangesAsync();
        }
    }
}
