using InventoryManagmentSystem.DTOs;

namespace InventoryManagmentSystem.Repositories.Interfaces
{
    public interface IRequestRepository
    {
        Task CreateRequest(AddRequest requestDetails);
    }
}
