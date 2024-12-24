using InventoryManagmentSystem.Data;
using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Models;
using InventoryManagmentSystem.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagmentSystem.Repositories.Classes
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly InventoryManagmentContext _context;

        public CategoryRepository(InventoryManagmentContext context)
        {
            _context = context;
        }

        public async Task AddCatAsync(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new ArgumentException("Category name cannot be null or empty.");
            }

            Category newCategory = new Category
            {
                Name = categoryName
            };

            await _context.Categories.AddAsync(newCategory);
            await _context.SaveChangesAsync();
        }

        public async Task<ICollection<GetCategoryDTO>> GetAllCategoriesAsync()
        {
            List<GetCategoryDTO> categories = await _context.Categories
                .AsNoTracking()
                .Select(category => new GetCategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name
                })
                .ToListAsync();

            if (!categories.Any())
            {
                throw new KeyNotFoundException("No categories found.");
            }

            return categories;
        }
    }
}