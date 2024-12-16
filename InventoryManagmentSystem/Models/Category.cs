using Azure.Core;

namespace InventoryManagmentSystem.Models
{
    public class Category : BaseClass
    {
        public ICollection<Product> Products { get; set; }
        public ICollection<Request> Requests { get; set; }
    }
}
