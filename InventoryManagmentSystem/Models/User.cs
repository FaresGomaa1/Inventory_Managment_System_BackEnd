using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagmentSystem.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ManagerId { get; set; }
        public User Manager { get; set; }
        public int TeamId { get; set; }
        public Team Team { get; set; }
        public ICollection<Request> Requests { get; set; }
        public ICollection<User> Subordinates { get; set; }
    }
}
