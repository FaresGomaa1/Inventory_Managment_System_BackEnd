using InventoryManagmentSystem.DTOs;

namespace InventoryManagmentSystem.Repositories.Interfaces
{
    public interface ISupplierRepository
    {
        Task<ICollection<GetSupplierDTO>> GetAllSuppliersAsync();
        Task<GetSupplierDTO> GetSupplierAsync(int id);
        Task AddSupplierAsync(AddSupplierDTO supplierDto);
    }
}
