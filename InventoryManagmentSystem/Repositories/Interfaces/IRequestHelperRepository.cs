using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Models;

namespace InventoryManagmentSystem.Repositories.Interfaces
{
    public interface IRequestHelperRepository
    {
        User GetUserById(string userId);
        Task CheckSKU(string newSKU);
        Task HandleAddRequestAsync(AddRequest requestDetails);
        Request CreateRequestFromDetails(AddRequest requestDetails, string userId, int? teamId);
        Request CreateRequestFromProduct(Product product, string userId, int? teamId, string requestType);
        ICollection<GetRequests> GetUserRequests(string userId, string sortBy, bool isAscending);
        ICollection<GetRequests> GetAllRequests(string sortBy, bool isAscending);
        ICollection<GetRequests> GetActiveRequests(string sortBy, bool isAscending);
        ICollection<GetRequests> GetInactiveRequests(string sortBy, bool isAscending);
        ICollection<GetRequests> GetTeamRequests(int teamId, string sortBy, bool isAscending);
        ICollection<GetRequests> SortRequests(IEnumerable<Request> requests, string sortBy = "Name", bool isAscending = true);
    }
}
