using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Models;

namespace InventoryManagmentSystem.Repositories.Interfaces
{
    public interface IRequestRepository
    {
        Task CreateRequest(AddRequest requestDetails);
        Task UpdateRequest(UpdateRequest updateRequest);
        Task DeleteProductRequest(string SKU, string userId);
        Task<bool> HasOnlyOneActiveRequestForProductAsync(string sku);
        Task<ICollection<GetRequests>> GetRequests(string viewName, string sortBy, string? userId, bool isAscending);
        Task<ICollection<UserRequestInfo>> GetUsersWithActiveRequestsCount(string managerId);
        Task AssignRequestToTeamMemberAsync(Assign assign);
        Task HandleManagerDecisionAsync(ManagerDecision managerDecision);
        Task<string> GenerateSKU(string SKU, string requestType);
        Task<GetRequests> GetRequestById(int id);
    }
}
