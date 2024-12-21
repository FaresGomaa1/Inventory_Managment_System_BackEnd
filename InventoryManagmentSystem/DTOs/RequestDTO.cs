using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagmentSystem.DTOs
{
    public class AddRequest
    {
       
        [Required]
        public string RequestType { get; set; }
        [Required]
        public string ProductName { get; set; }
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
        public int CategoryId { get; set; }
        [Required]
        public int SupplierId { get; set; }
        public string? UserId { get; set; }
    }
    public class UpdateRequest: AddRequest
    {
        [Required]
        public int RequestId { get; set; }
        [Required]
        public string RquestStatus { get; set; }
    }
}
