using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagmentSystem.Models
{
    public class Request : BaseClass
    {
        [Required]
        public bool Status { get; set; }
        [Required]
        public string RquestStatus {  get; set; }
        [Required]
        public string RequestType { get; set; }
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
        public DateTime CreatedOn { get; set; }
        public string? InventoryManagerDecision { get; set; }
        [MaxLength(500)]
        public string? InventoryManagerComment { get; set; }
        public string? DepartmentManagerDecision { get; set; }
        [MaxLength(500)]
        public string? DepartmentManagerComment { get; set; }
        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        [Required]
        [ForeignKey("Supplier")]
        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }
        [ForeignKey("User")]
        public string? UserId { get; set; }
        public virtual User User { get; set; }
        [ForeignKey("Team")]
        public int? TeamId { get; set; }
        public virtual Team Team { get; set; }
    }
}
