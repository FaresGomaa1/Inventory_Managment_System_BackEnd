﻿using InventoryManagmentSystem.Data;
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
        private readonly IProductRepository _productRepository;

        // Inject dependencies into the repository
        public RequestRepository(InventoryManagmentContext context, UserManager<User> userManager, IRequestHelperRepository requestHelperRepository, IProductRepository productRepository)  
        {
            _context = context;
            _userManager = userManager;
            _requestHelperRepository = requestHelperRepository;
            _productRepository = productRepository;
        }

        public async Task<GetRequests> GetRequestById(int id)
        {
            // Try to fetch the request from the database and handle the case where not found.
            Request request = await _context.Requests
                .Include(r => r.User)
                .Include(r => r.Category)
                .Include(r => r.Supplier)
                .Include(r => r.Team)
                .FirstOrDefaultAsync(r => r.Id == id) ?? throw new KeyNotFoundException($"Request with id {id} not found.");

            // Map the Request entity to a DTO or view model
            return new GetRequests()
            {
                Id = request.Id,
                RequestType = request.RequestType,
                Name = request.Name,
                Price = request.Price,
                SKU = request.SKU,
                Quantity = request.Quantity,
                Description = request.Description,
                Status = request.Status,
                InventoryManagerComment = request.InventoryManagerComment,
                InventoryManagerDecision = request.InventoryManagerDecision,
                DepartmentManagerComment = request.DepartmentManagerComment,
                DepartmentManagerDecision = request.DepartmentManagerDecision,
                RequestStatus = request.RequestStatus,
                CreatedOn = request.CreatedOn,
                Category = request.Category?.Name ?? "No Catefory foumd",
                CategoryId = request.CategoryId,
                Supplier = $"{request.Supplier?.FirstName} {request.Supplier?.LastName}",
                SupplierId = request.SupplierId,
                User = $"{request.User?.FirstName} {request.User?.LastName}",
                UserId = request.UserId ?? "not found",
                Team = request.Team?.Name ?? "No Team foumd",
                TeamId = request.Team.Id,
            };
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
            await _context.Requests.AddAsync(await newRequest);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRequest(UpdateRequest updateRequest)
        {
            // Fetch the user details
            User user = await _requestHelperRepository.GetUserById(updateRequest.UserId);
            // Fetch the request to be updated
            Request request = await _context.Requests.FirstOrDefaultAsync(r => r.Id == updateRequest.RequestId) ?? throw new KeyNotFoundException($"There is no Request with Id {updateRequest.RequestId}");

            // Update request properties
            request.Description = updateRequest.Description;
            request.Name = updateRequest.Name;
            request.Price = updateRequest.Price;
            request.Quantity = updateRequest.Quantity;
            request.CategoryId = updateRequest.CategoryId;
            request.SupplierId = updateRequest.SupplierId;
            request.RequestStatus = "Updated";
            request.TeamId = 2;
            request.UserId = null;

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
            _context.Requests.Add(await newRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasOnlyOneActiveRequestForProductAsync(string sku)
        {
            int activeRequestCount = await _context.Requests
                .Where(r => r.SKU == sku && r.Status == true)
                .CountAsync();
            return activeRequestCount == 1;
        }

        public async Task<ICollection<GetRequests>> GetRequests(string viewName, string sortBy, string? userId, bool isAscending)
        {
            // Ensure sortBy is not null or empty
            if (string.IsNullOrEmpty(sortBy))
                throw new ArgumentException("Sort criteria cannot be null or empty.", nameof(sortBy));
            User user =  _context.Users.FirstOrDefault(u => u.Id == userId);
            // Dictionary to handle common views
            var requestViews = new Dictionary<string, Func<Task<ICollection<GetRequests>>>>()
    {
        { "My Request", () => _requestHelperRepository.GetUserRequests(userId, sortBy, isAscending) },
        { "All Requests", () => _requestHelperRepository.GetAllRequests(sortBy, isAscending) },
        { "Active Requests", () => _requestHelperRepository.GetActiveRequests(sortBy, isAscending) },
        { "Inactive Requests", () => _requestHelperRepository.GetInactiveRequests(sortBy, isAscending) },
        { "Team Requests", () => _requestHelperRepository.GetTeamRequests(user.TeamId,sortBy, isAscending) },
    };

            // Check if the view name exists in the dictionary
            if (requestViews.ContainsKey(viewName))
            {
                return await requestViews[viewName]();
            }

            // For "Team Requests", retrieve the user and use TeamId for filtering
            if (viewName == "Team Requests")
            {
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");

                return await _requestHelperRepository.GetTeamRequests(user.TeamId, sortBy, isAscending);
            }

            // Default case: fetch all requests with the associated entities
            IEnumerable<Request> requests = _context.Requests
                .Include(r => r.Category)
                .Include(r => r.Supplier)
                .Include(r => r.User)
                .Include(r => r.Team)
                .ToList();

            // Sort the requests and return as GetRequests
            return await _requestHelperRepository.SortRequests(requests);
        }

        public async Task<ICollection<UserRequestInfo>> GetUsersWithActiveRequestsCount(string managerId)
        {
            var usersWithRequests = _context.Users
                .Where(u => u.ManagerId == managerId)
                .Include(u => u.Requests)
                .ToList();

            var activeRequestIds = _context.Requests
                .Where(r => r.Status)
                .Select(r => r.Id)
                .ToHashSet();

            var result = usersWithRequests
                .Select(user => new UserRequestInfo
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    ActiveRequestCount = user.Requests.Count(r => activeRequestIds.Contains(r.Id))
                })
                .OrderBy(info => info.ActiveRequestCount)
                .ToList();

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
            if (await IsRejectionDecision(managerDecision.Decision) && string.IsNullOrWhiteSpace(managerDecision.Comment))
                throw new ArgumentException("A comment is required when rejecting the decision.");

            User user = await _userManager.FindByIdAsync(managerDecision.ManagerId) ?? throw new KeyNotFoundException($"User with Id {managerDecision.ManagerId} not found.");
            Request request = await _context.Requests.FirstOrDefaultAsync(r => r.Id == managerDecision.RequestId) ?? throw new KeyNotFoundException($"Request with Id {managerDecision.RequestId} not found.");

            ValidateUserPermissionsForRequest(user, request);

            if (managerDecision.managerType.Equals("InventoryManager", StringComparison.OrdinalIgnoreCase))
            {
                request.InventoryManagerDecision = managerDecision.Decision;
                request.InventoryManagerComment = managerDecision.Comment;
                request = await ChangeRequestStatus(request.InventoryManagerDecision, request, "InventoryManager");
            }
            else if (managerDecision.managerType.Equals("DepartmentManager", StringComparison.OrdinalIgnoreCase))
            {
                if (await IsRejectionDecision(request.InventoryManagerDecision))
                    throw new InvalidOperationException($"The decision cannot be added because the request with ID {request.Id} is not in your stage.");
                

                request.DepartmentManagerDecision = managerDecision.Decision;
                request.DepartmentManagerComment = managerDecision.Comment;
                request = await ChangeRequestStatus(request.InventoryManagerDecision, request, "DepartmentManager");
            }
            else
            {
                throw new ArgumentException($"Invalid manager type: {managerDecision.managerType}. Expected 'InventoryManager' or 'DepartmentManager'.");
            }

            await _context.SaveChangesAsync();
        }

        private async Task ValidateUserPermissionsForRequest(User user, Request request)
        {
            //if (!string.IsNullOrEmpty(request.UserId))
            //    throw new InvalidOperationException($"Request with Id {request.Id} is assigned to another user. It cannot be accessed by user with Id {user.Id}.");

            if (user.TeamId != request.TeamId)
                throw new UnauthorizedAccessException($"User with Id {user.Id} does not have permission to access Request with Id {request.Id} as it belongs to a different team.");

            if (!request.Status)
                throw new InvalidOperationException($"Request with Id {request.Id} is inactive.");
        }

        private async Task<bool> IsRejectionDecision(string decision)
        {
            return decision == "Reject - Update" || decision == "Reject - Close";
        }

        private  async Task<Request> ChangeRequestStatus(string managerDecision, Request request, string managerType)
        {
            if (await IsRejectionDecision(managerDecision))
            {
                HandleRejectionDecision(managerDecision, request);
            }
            else if (managerDecision == "Approve")
            {
                HandleApprovalDecision(managerType, request);
            }

            return request;
        }

        private async Task HandleRejectionDecision(string managerDecision, Request request)
        {
            switch (managerDecision)
            {
                case "Reject - Update":
                    request.RequestStatus = "Reject - Update";
                    request.UserId = null;
                    request.TeamId = 1;
                    break;

                case "Reject - Close":
                    request.RequestStatus = "Reject - Close";
                    request.Status = false;
                    break;
            }
        }

        private async Task HandleApprovalDecision(string managerType, Request request)
        {
            switch (managerType)
            {
                case "InventoryManager":
                    request.UserId = null;
                    request.RequestStatus = "Approved By Inventory Manager";
                    request.TeamId = 3;
                    break;

                case "DepartmentManager":
                    if (request.RequestType == "Update Request")
                    {
                        UpdateProductDTO updateProductDTO = new UpdateProductDTO()
                        {
                            Name = request.Name,
                            Price = request.Price,
                            SKU = request.SKU,
                            Quantity = request.Quantity,
                            Description = request.Description,
                            CategoryId = request.CategoryId,
                            SupplierId = request.SupplierId
                        };
                        await _productRepository.UpdateProductAsync(updateProductDTO);
                    }
                    else if (request.RequestType == "Add Request")
                    {
                        AddProductDTO addProductDTO = new AddProductDTO()
                        {
                            Name = request.Name,
                            Price = request.Price,
                            SKU = request.SKU,
                            Quantity = request.Quantity,
                            Description = request.Description,
                            CategoryId = request.CategoryId,
                            SupplierId = request.SupplierId,
                            Created_On = DateTime.Now,
                        };
                        await _productRepository.AddProductAsync(addProductDTO);
                    }
                    else if(request.RequestType == "Delete Request")
                    {
                        await _productRepository.DeleteProductAsync(request.SKU);
                    }
                    request.RequestStatus = "Published";
                    request.Status = false;
                    break;

            }
        }
        public async Task<string> GenerateSKU(string SKU, string requestType)
        {
            if (requestType == "Update Request")
            {
                Product existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.SKU == SKU);
                if (existingProduct != null && existingProduct.SKU != SKU)
                {
                    string SKUfirstTwoLetter = SKU.Substring(0, 2);
                    int i = 0;
                    string trySku = SKU;
                    while (existingProduct != null && existingProduct.SKU != trySku)
                    {
                         trySku = $"{SKUfirstTwoLetter}-{i}";
                        existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.SKU == trySku);
                        i++;
                    }
                    return trySku;
                }
            }
            else if (requestType == "Add Request")
            {
                Product existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.SKU == SKU);
                Request existingRequest = await _context.Requests.FirstOrDefaultAsync(r=>r.SKU == SKU);
                if (existingProduct != null || existingRequest != null)
                {
                    string SKUfirstTwoLetter = SKU.Substring(0, 2);
                    int i = 0;
                    string trySku = SKU;
                    while (existingProduct != null || existingRequest != null)
                    {
                         trySku = $"{SKUfirstTwoLetter}-{i}";
                        existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.SKU == trySku);
                         existingRequest = await _context.Requests.FirstOrDefaultAsync(r => r.SKU == trySku);
                        i++;
                    }
                    return trySku;
                }
            }
            return SKU;
        }

    }
}