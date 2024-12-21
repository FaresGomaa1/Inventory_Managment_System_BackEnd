﻿using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Models;

namespace InventoryManagmentSystem.Repositories.Interfaces
{
    public interface IHelperRepository
    {
        Task<string> GenerateJwtToken(User user);
        Task<bool> CheckEmailAndUserName(string email, string username);
        Task AssignRolesToUser(string role, User user);
        Task EnsureRoleExistsAndAssign(string roleName, User user);
        Task ValidateUserDetails(RegisterDTO model);
        Task<string?> AssignManager(RegisterDTO model, int userTeamId);
        int AssignTeamId(string role);
    }
}
