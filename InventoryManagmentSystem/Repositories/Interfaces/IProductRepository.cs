using InventoryManagmentSystem.DTOs;

namespace InventoryManagmentSystem.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task AddProductAsync(AddProductDTO addProductDTO);
        Task UpdateProductAsync(UpdateProductDTO updateProductDTO);
        Task<ICollection<GetProductDTO>> GetAllProductsAsync();
        Task<GetProductDTO> GetProductAsync(string sku);
        Task DeleteProductAsync(string sku);
    }
}
