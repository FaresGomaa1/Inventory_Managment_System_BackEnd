﻿using System.ComponentModel.DataAnnotations;

namespace InventoryManagmentSystem.DTOs
{
    public class RegisterDTO
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public bool IsManager { get; set; }
        public string ManagerId { get; set; }
    }
    public class LoginDTO
    {
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string UserName { get; set; }
    }
}