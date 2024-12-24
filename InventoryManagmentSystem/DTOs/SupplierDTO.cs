using System.ComponentModel.DataAnnotations;

namespace InventoryManagmentSystem.DTOs
{
    public class GetSupplierDTO : AddSupplierDTO
    {
        public int Id { get; set; }
    }
    public class AddSupplierDTO
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Address { get; set; }
    }
}
