using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace InventoryManagmentSystem.DTOs
{
    public class AddRequest
    {

        [Required]
        public string RequestType { get; set; }
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
        public int CategoryId { get; set; }
        [Required]
        public int SupplierId { get; set; }
        public string? UserId { get; set; }
    }
    public class UpdateRequest : AddRequest
    {
        [Required]
        public int RequestId { get; set; }
        [Required]
        public string RequestStatus { get; set; }
    }
    public class GetRequests
    {
        public int Id { get; set; }
        public string RequestType { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string SKU { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public string? InventoryManagerDecision { get; set; }
        public string? InventoryManagerComment { get; set; }
        public string? DepartmentManagerDecision { get; set; }
        public string? DepartmentManagerComment { get; set; }
        public string RequestStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Category { get; set; }
        public int CategoryId { get; set; }
        public string Supplier { get; set; }
        public int SupplierId { get; set; }
        public string User { get;set; }
        public string UserId { get; set; }  
        public string Team { get; set; }
        public int TeamId { get; set; } 
    }

}