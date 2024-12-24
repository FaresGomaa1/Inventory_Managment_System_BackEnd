using System.ComponentModel.DataAnnotations;

namespace InventoryManagmentSystem.DTOs
{
    public class GetProductDTO: BaseClass
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string SupplierFullName { get; set; }
        public int SupplierId { get; set; }
    }
    public class AddProductDTO : BaseClass
    {
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public int SupplierId { get; set; }
    }
    public class UpdateProductDTO : AddProductDTO
    {
        public int Id { get; set; }
    }
    public class BaseClass
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public string SKU { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        [MaxLength(500)]
        public string Description { get; set; }
        [Required]
        public DateTime Created_On { get; set; }
    }
}
