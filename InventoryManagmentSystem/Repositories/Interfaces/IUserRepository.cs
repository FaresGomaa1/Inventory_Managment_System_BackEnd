using InventoryManagmentSystem.DTOs;

namespace InventoryManagmentSystem.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<(bool IsSuccess, string Message, string Token)> LoginUserAsync(LoginDTO model);
        Task<string> CreateUser(RegisterDTO model);
    }
}
