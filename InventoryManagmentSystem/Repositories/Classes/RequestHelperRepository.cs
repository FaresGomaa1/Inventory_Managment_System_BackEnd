using InventoryManagmentSystem.Data;
using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Models;
using InventoryManagmentSystem.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagmentSystem.Repositories.Classes
{
    public class RequestHelperRepository: IRequestHelperRepository
    {
        private readonly InventoryManagmentContext _context;
        private readonly UserManager<User> _userManager;

        // Inject dependencies into the repository
        public RequestHelperRepository(InventoryManagmentContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // Utility method to fetch a user by ID
        public User GetUserById(string userId)
        {
            User user = _userManager.FindByIdAsync(userId).Result;
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found.");
            }
            return user;
        }
        public async Task CheckSKU(string newSKU)
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(r => r.SKU == newSKU);
            if (existingProduct != null && newSKU != existingProduct.SKU)
            {
                throw new InvalidOperationException($"SKU '{newSKU}' already exists in the system.");
            }
        }
        public async Task HandleAddRequestAsync(AddRequest requestDetails)
        {
            // Check if the SKU already exists
            bool skuExists = await _context.Products.AnyAsync(r => r.SKU == requestDetails.SKU);
            if (skuExists)
            {
                // Log and throw exception if SKU already exists
                throw new InvalidOperationException("SKU already exists in the system.");
            }
        }
        public Request CreateRequestFromProduct(Product product,string teamName)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            Team inventoryManagerTeam = _context.Teams.FirstOrDefault(t => t.Name == teamName) ?? throw new KeyNotFoundException($"There is no team with name {teamName}");

            return new Request
            {
                SKU = product.SKU,
                Price = product.Price,
                Name = product.Name,
                Quantity = product.Quantity,
                Description = product.Description,
                CategoryId = product.CategoryId,
                SupplierId = product.SupplierId,
                TeamId = inventoryManagerTeam.Id,
                RequestType = "Delete Request",
                CreatedOn = DateTime.UtcNow,
                Status = true,
                RquestStatus = "New Request"
            };
        }
        public Request CreateRequestFromDetails(AddRequest requestDetails, string teamName)
        {
            if (requestDetails == null) throw new ArgumentNullException(nameof(requestDetails));
            Team inventoryManagerTeam = _context.Teams.FirstOrDefault(t => t.Name == teamName) ?? throw new KeyNotFoundException($"There is no team with name {teamName}");

            return new Request
            {
                SKU = requestDetails.SKU,
                Name = requestDetails.Name,
                Price = requestDetails.Price,
                Quantity = requestDetails.Quantity,
                Description = requestDetails.Description,
                CategoryId = requestDetails.CategoryId,
                SupplierId = requestDetails.SupplierId,
                TeamId = inventoryManagerTeam.Id,
                RequestType = requestDetails.RequestType,
                CreatedOn = DateTime.UtcNow,
                Status = true,
                RquestStatus = "New Request"
            };
        }

        // Retrieves user-specific requests and sorts them
        public ICollection<GetRequests> GetUserRequests(string userId, string sortBy, bool isAscending)
        {
            var requests = _context.Requests
                .Where(r => r.UserId == userId)
                .Include(r => r.Category)
                .Include(r => r.Supplier) 
                .Include(r => r.User) 
                .Include(r => r.Team)
                .ToList();

            return SortRequests(requests, sortBy, isAscending);
        }

        // Retrieves all requests and sorts them
        public ICollection<GetRequests> GetAllRequests(string sortBy, bool isAscending)
        {
            var requests = _context.Requests
                .Include(r => r.Category)
                .Include(r => r.Supplier)
                .Include(r => r.User)
                .Include(r => r.Team).
                ToList();
            return SortRequests(requests, sortBy, isAscending);
        }

        // Retrieves only active requests and sorts them
        public ICollection<GetRequests> GetActiveRequests(string sortBy, bool isAscending)
        {
            var activeRequests = _context.Requests
                .Where(r => r.Status == true)
                .Include(r => r.Category)
                .Include(r => r.Supplier)
                .Include(r => r.User)
                .Include(r => r.Team)
                .ToList();

            return SortRequests(activeRequests, sortBy, isAscending);
        }

        // Retrieves only inactive requests and sorts them
        public ICollection<GetRequests> GetInactiveRequests(string sortBy, bool isAscending)
        {
            var inactiveRequests = _context.Requests
                .Where(r => r.Status == false)
                .Include(r => r.Category)
                .Include(r => r.Supplier)
                .Include(r => r.User)
                .Include(r => r.Team)
                .ToList();

            return SortRequests(inactiveRequests, sortBy, isAscending);
        }

        // Retrieves team-specific requests and sorts them
        public ICollection<GetRequests> GetTeamRequests(int teamId, string sortBy, bool isAscending)
        {
            var teamRequests = _context.Requests
                .Where(r => r.TeamId == teamId && string.IsNullOrEmpty(r.UserId))
                .Include(r => r.Category)
                .Include(r => r.Supplier)
                .Include(r => r.User)
                .Include(r => r.Team)
                .ToList();

            return SortRequests(teamRequests, sortBy, isAscending);
        }
        // Helper method to sort requests based on a criteria
        public ICollection<GetRequests> SortRequests(IEnumerable<Request> requests, string sortBy = "Name", bool isAscending = true)
        {
            if (requests == null)
                throw new ArgumentNullException(nameof(requests), "Requests cannot be null");

            if (string.IsNullOrEmpty(sortBy))
                throw new ArgumentException("Sort criteria cannot be null or empty.", nameof(sortBy));

            // Define a dynamic sorting key
            Func<Request, object> keySelector = sortBy switch
            {
                "Name" => r => r.Name,
                "Status" => r => r.Status,
                "Price" => r => r.Price,
                "SKU" => r => r.SKU,
                "Quantity" => r => r.Quantity,
                "Team" => r => r.Team?.Name,
                "Category" => r => r.Category?.Name,  // Null check for Category
                "Supplier First Name" => r => r.Supplier?.FirstName,  // Null check for Supplier
                "Supplier Last Name" => r => r.Supplier?.LastName,  // Null check for Supplier
                "User First Name" => r => r.User?.FirstName,  // Null check for User
                "User Last Name" => r => r.User?.LastName,  // Null check for User
                "CreatedOn" => r => r.CreatedOn,
                _ => throw new ArgumentException($"Invalid sort criteria: {sortBy}", nameof(sortBy))
            };

            // Apply sorting
            requests = isAscending
                ? requests.OrderBy(keySelector)
                : requests.OrderByDescending(keySelector);

            // Convert to List of GetRequests using LINQ to improve readability and performance
            var result = requests.Select(r => new GetRequests
            {
                Status = r.Status,
                Id = r.Id,
                Name = r.Name,
                RquestStatus = r.RquestStatus,
                RequestType = r.RequestType,
                Price = r.Price,
                SKU = r.SKU,
                Quantity = r.Quantity,
                Description = r.Description,
                CreatedOn = r.CreatedOn,
                Category = r.Category?.Name,  // Null check for Category
                Supplier = $"{r.Supplier?.FirstName} {r.Supplier?.LastName}",  // Null check for Supplier
                User = $"{r.User?.FirstName} {r.User?.LastName}",  // Null check for User
                Team = r.Team?.Name,  // Null check for Team
            }).ToList();

            return result;
        }



    }
}
