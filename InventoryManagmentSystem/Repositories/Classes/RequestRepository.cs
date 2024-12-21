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
        private readonly IRequestHelperRepository _requestHelperRepository;


        // Inject dependencies into the repository
        public RequestRepository(InventoryManagmentContext context, UserManager<User> userManager, IRequestHelperRepository requestHelperRepository)
        {
            _context = context;
            _userManager = userManager;
            _requestHelperRepository = requestHelperRepository;
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
                    await _requestHelperRepository.HandleAddRequestAsync(requestDetails);
                    break;

                case "Update Request":
                    await _requestHelperRepository.CheckSKU(requestDetails.SKU);
                    break;

                default:
                    throw new ArgumentException($"Unsupported request type: {requestDetails.RequestType}", nameof(requestDetails.RequestType));
            }
            // Fetch the user details
            User user = _requestHelperRepository.GetUserById(requestDetails.UserId);

            // Create a new request based on the provided details
            var newRequest = _requestHelperRepository.CreateRequestFromDetails(requestDetails, user.Id, user.TeamId);

            // Save the new request to the database
            await _context.Requests.AddAsync(newRequest);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRequest(UpdateRequest updateRequest)
        {
            // Fetch the user details
            User user = _requestHelperRepository.GetUserById(updateRequest.UserId);
            // Fetch the request to be updated
            var request = await _context.Requests.FirstOrDefaultAsync(r => r.Id == updateRequest.RequestId);
            if (request == null)
            {
                throw new InvalidOperationException("Request not found");
            }
            await _requestHelperRepository.CheckSKU(updateRequest.SKU);
            // Update request properties
            request.Description = updateRequest.Description;
            request.Name = updateRequest.Name;
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
            var user = _requestHelperRepository.GetUserById(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found.");
            }

            // Create a new request object based on the existing product
            var newRequest = _requestHelperRepository.CreateRequestFromProduct(existingProduct, userId, user.TeamId, "Delete Request");
            _context.Requests.Add(newRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasOnlyOneActiveRequestForProductAsync(string sku)
        {
            int activeRequestCount = await _context.Requests
                .Where(r => r.SKU == sku && r.Status == true)
                .CountAsync();
            return activeRequestCount == 1;
        }
        public ICollection<GetRequests> GetRequests(string viewName, string sortBy, string? userId, bool isAscending)
        {
            // Ensure sortBy is not null or empty
            if (string.IsNullOrEmpty(sortBy))
                throw new ArgumentException("Sort criteria cannot be null or empty.", nameof(sortBy));

            // Dictionary to handle common views
            var requestViews = new Dictionary<string, Func<ICollection<GetRequests>>>()
    {
        { "My Request", () => _requestHelperRepository.GetUserRequests(userId, sortBy, isAscending) },
        { "All Requests", () => _requestHelperRepository.GetAllRequests(sortBy, isAscending) },
        { "Active Requests", () => _requestHelperRepository.GetAllRequests(sortBy, isAscending) },
        { "Inactive Requests", () => _requestHelperRepository.GetInactiveRequests(sortBy, isAscending) },
    };

            // Check if the view name exists in the dictionary
            if (requestViews.ContainsKey(viewName))
            {
                return requestViews[viewName]();
            }

            // For "Team Requests", retrieve the user and use TeamId for filtering
            if (viewName == "Team Requests")
            {
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");

                User user = _requestHelperRepository.GetUserById(userId);
                return _requestHelperRepository.GetTeamRequests(user.TeamId, sortBy, isAscending);
            }

            // Default case: fetch all requests with the associated entities
            IEnumerable<Request> requests = _context.Requests
                .Include(r => r.Category)
                .Include(r => r.Supplier)
                .Include(r => r.User)
                .Include(r => r.Team)
                .ToList();

            // Sort the requests and return as GetRequests
            return _requestHelperRepository.SortRequests(requests);
        }

    }
}