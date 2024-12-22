using System.ComponentModel.DataAnnotations;

namespace InventoryManagmentSystem.DTOs
{
    public class UserRequestInfo
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ActiveRequestCount { get; set; }
    }
    public class ManagerDecision
    {
        [Required]
        public string ManagerId { get; set; }
        [Required]
        public int RequestId { get; set; }
        [Required]
        public string Decision { get; set; }
        public string Comment { get; set; }
        [Required]
        public string managerType { get; set; }
    }
    public class Assign
    {
        [Required]
        public string userId { get; set; }
        [Required]
        public string managerId { get; set; }
        [Required]
        public int requestId { get; set; }

    }
}
