using System.ComponentModel.DataAnnotations;

namespace InventoryManagmentSystem.Models
{
    public class BaseClass
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
