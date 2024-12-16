using System.ComponentModel.DataAnnotations;

namespace InventoryManagmentSystem.Models
{
    public class Product : BaseClass
    {
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
        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        [Required]
        public int SupplierId {  get; set; }
        public Supplier Supplier { get; set; }

    }
}
