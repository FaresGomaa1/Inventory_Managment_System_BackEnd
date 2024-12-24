using InventoryManagmentSystem.DTOs;

namespace InventoryManagmentSystem.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task AddProductAsync(AddProductDTO addProductDTO);
        Task UpdateProductAsync(UpdateProductDTO updateProductDTO);
        Task<ICollection<GetProductDTO>> GetAllProductsAsync();
        Task<GetProductDTO> GetProductAsync(int id);
        Task DeleteProductAsync(int id);
    }
}
