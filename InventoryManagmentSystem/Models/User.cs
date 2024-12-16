using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagmentSystem.Models
{
    public class User : IdentityUser
    {
        public int TeamId { get; set; }
        public Team Team { get; set; }
        public ICollection<Request> Requests { get; set; }
    }
}
