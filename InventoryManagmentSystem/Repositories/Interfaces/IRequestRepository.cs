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
    }
}
