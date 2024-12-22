using InventoryManagmentSystem.Data;
using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Models;
using InventoryManagmentSystem.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

            // Create a new request based on the provided details
            var newRequest = _requestHelperRepository.CreateRequestFromDetails(requestDetails, "Inventory Manager");

            // Save the new request to the database
            await _context.Requests.AddAsync(newRequest);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRequest(UpdateRequest updateRequest)
        {
            // Fetch the user details
            User user = _requestHelperRepository.GetUserById(updateRequest.UserId);
            // Fetch the request to be updated
            Request request = await _context.Requests.FirstOrDefaultAsync(r => r.Id == updateRequest.RequestId) ?? throw new KeyNotFoundException($"There is no Request with Id {updateRequest.RequestId}");

            await _requestHelperRepository.CheckSKU(updateRequest.SKU);
            // Update request properties
            request.Description = updateRequest.Description;
            request.Name = updateRequest.Name;
            request.Price = updateRequest.Price;
            request.Quantity = updateRequest.Quantity;
            request.CategoryId = updateRequest.CategoryId;
            request.SupplierId = updateRequest.SupplierId;
            if (request.RquestStatus == "Reject - Update")
                request.RquestStatus = "Updated";

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
            var newRequest = _requestHelperRepository.CreateRequestFromProduct(existingProduct, "Inventory Manager");
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

        public ICollection<UserRequestInfo> GetUsersWithActiveRequestsCount(string managerId)
        {
            List<User> users = _context.Users
                .Where(u => u.ManagerId == managerId)
                .Include(u => u.Requests)
                .ToList();

            List<Request> activeRequests = _context.Requests
                .Where(r => r.Status)
                .ToList();

            List<UserRequestInfo> result = new List<UserRequestInfo>();

            HashSet<int> activeRequestIds = new HashSet<int>(activeRequests.Select(r => r.Id));

            foreach (User user in users)
            {
                int activeRequestCount = user.Requests.Count(r => activeRequestIds.Contains(r.Id));

                result.Add(new UserRequestInfo
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    ActiveRequestCount = activeRequestCount
                });
            }

            return result;
        }

        public async Task AssignRequestToTeamMemberAsync(Assign assign)
        {
            User user = await _userManager.FindByIdAsync(assign.userId) ?? throw new KeyNotFoundException($"User with Id {assign.userId} not found.");

            if (user.ManagerId != assign.managerId)
                throw new UnauthorizedAccessException($"User with Id {assign.userId} does not belong to manager with Id {assign.managerId}.");

            Request request = await _context.Requests.FirstOrDefaultAsync(r => r.Id == assign.requestId) ?? throw new KeyNotFoundException($"Request with Id {assign.requestId} not found.");

            ValidateUserPermissionsForRequest(user, request);

            request.UserId = user.Id;

            await _context.SaveChangesAsync();
        }

        public async Task HandleManagerDecisionAsync(ManagerDecision managerDecision)
        {
            if (IsRejectionDecision(managerDecision.Decision) && string.IsNullOrWhiteSpace(managerDecision.Comment))
                throw new ArgumentException("A comment is required when rejecting the decision.");

            User user = await _userManager.FindByIdAsync(managerDecision.ManagerId) ?? throw new KeyNotFoundException($"User with Id {managerDecision.ManagerId} not found.");
            Request request = await _context.Requests.FirstOrDefaultAsync(r => r.Id == managerDecision.RequestId) ?? throw new KeyNotFoundException($"Request with Id {managerDecision.RequestId} not found.");

            ValidateUserPermissionsForRequest(user, request);

            if (managerDecision.managerType.Equals("InventoryManager", StringComparison.OrdinalIgnoreCase))
            {
                request.InventoryManagerDecision = managerDecision.Decision;
                request.InventoryManagerComment = managerDecision.Comment;
                request = ChangeRequestStatus(request.InventoryManagerDecision, request, "InventoryManager");
            }
            else if (managerDecision.managerType.Equals("DepartmentManager", StringComparison.OrdinalIgnoreCase))
            {
                if (IsRejectionDecision(request.InventoryManagerDecision))
                    throw new InvalidOperationException($"The decision cannot be added because the request with ID {request.Id} is not in your stage.");
                

                request.DepartmentManagerDecision = managerDecision.Decision;
                request.DepartmentManagerComment = managerDecision.Comment;
                request = ChangeRequestStatus(request.InventoryManagerDecision, request, "DepartmentManager");
            }
            else
            {
                throw new ArgumentException($"Invalid manager type: {managerDecision.managerType}. Expected 'InventoryManager' or 'DepartmentManager'.");
            }

            await _context.SaveChangesAsync();
        }

        private void ValidateUserPermissionsForRequest(User user, Request request)
        {
            if (user.TeamId != request.TeamId)
                throw new UnauthorizedAccessException($"User with Id {user.Id} does not have permission to access Request with Id {request.Id} as it belongs to a different team.");

            if (!request.Status)
                throw new InvalidOperationException($"Request with Id {request.Id} is inactive.");

            if (user.Id != request.UserId)
                throw new InvalidOperationException($"Request with Id {request.Id} is assigned to another user. It cannot be accessed by user with Id {user.Id}.");
        }
        private bool IsRejectionDecision(string decision)
        {
            return decision == "Reject - Update" || decision == "Reject - Close";
        }
        private Request ChangeRequestStatus(string managerDecision, Request request, string managerType)
        {
            if (IsRejectionDecision(managerDecision))
            {
                HandleRejectionDecision(managerDecision, request);
            }
            else if (managerDecision == "Approved")
            {
                HandleApprovalDecision(managerType, request);
            }

            return request;
        }
        private void HandleRejectionDecision(string managerDecision, Request request)
        {
            switch (managerDecision)
            {
                case "Reject - Update":
                    request.RquestStatus = "Reject - Update";
                    request.UserId = string.Empty;
                    request.TeamId = 1;
                    break;

                case "Reject - Close":
                    request.RquestStatus = "Reject - Close";
                    request.Status = false;
                    break;
            }
        }
        private void HandleApprovalDecision(string managerType, Request request)
        {
            switch (managerType)
            {
                case "InventoryManager":
                    request.UserId = string.Empty;
                    request.RquestStatus = "Approved By Inventory Manager";
                    request.TeamId = 3;
                    break;

                case "DepartmentManager":
                    request.RquestStatus = "Published";
                    break;
            }
        }
    }
}