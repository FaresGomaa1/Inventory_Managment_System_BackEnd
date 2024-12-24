using InventoryManagmentSystem.DTOs;

namespace InventoryManagmentSystem.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<ICollection<GetCategoryDTO>> GetAllCategoriesAsync();
        Task AddCatAsync(string categoryName);
    }
}
