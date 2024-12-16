using Azure.Core;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagmentSystem.Models
{
    public class Supplier : BaseClass
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Address { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<Request> Requests { get; set; }
    }
}
