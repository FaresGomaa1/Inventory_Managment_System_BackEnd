using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Models;

namespace InventoryManagmentSystem.Repositories.Interfaces
{
    public interface IRequestHelperRepository
    {
        Task<User> GetUserById(string userId);
        Task CheckSKU(string newSKU);
        Task HandleAddRequestAsync(AddRequest requestDetails);
        Task<Request> CreateRequestFromDetails(AddRequest requestDetails, string teamName);
        Task<Request> CreateRequestFromProduct(Product product, string teamName);
        Task<ICollection<GetRequests>> GetUserRequests(string userId, string sortBy, bool isAscending);
        Task<ICollection<GetRequests>> GetAllRequests(string sortBy, bool isAscending);
        Task<ICollection<GetRequests>> GetActiveRequests(string sortBy, bool isAscending);
        Task<ICollection<GetRequests>> GetInactiveRequests(string sortBy, bool isAscending);
        Task<ICollection<GetRequests>> GetTeamRequests(int teamId, string sortBy, bool isAscending);
        Task<ICollection<GetRequests>> SortRequests(IEnumerable<Request> requests, string sortBy = "Name", bool isAscending = true);
    }
}
