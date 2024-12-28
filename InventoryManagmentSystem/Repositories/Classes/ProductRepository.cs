using InventoryManagmentSystem.Data;
using InventoryManagmentSystem.DTOs;
using InventoryManagmentSystem.Models;
using InventoryManagmentSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagmentSystem.Repositories.Classes
{
    public class ProductRepository : IProductRepository
    {
        private readonly InventoryManagmentContext _context;
        private readonly IRequestHelperRepository _requestHelperRepository;

        public ProductRepository(InventoryManagmentContext context, IRequestHelperRepository requestHelperRepository)
        {
            _context = context;
            _requestHelperRepository = requestHelperRepository ?? throw new Exception("An error occurred while initializing IRequestHelperRepository.");
        }

        public async Task AddProductAsync(AddProductDTO addProductDTO)
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.SKU == addProductDTO.SKU);
            if (existingProduct != null)
            {
                throw new InvalidOperationException($"Product with SKU '{existingProduct.SKU}' already exists in the system.");
            }
            if (addProductDTO == null)
            {
                throw new ArgumentNullException(nameof(addProductDTO), "Product data cannot be null.");
            }

            Product newProduct = new Product
            {
                Name = addProductDTO.Name,
                Price = addProductDTO.Price,
                SKU = addProductDTO.SKU,
                Quantity = addProductDTO.Quantity,
                Description = addProductDTO.Description,
                CategoryId = addProductDTO.CategoryId,
                SupplierId = addProductDTO.SupplierId
            };

            await _context.Products.AddAsync(newProduct);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id) ?? throw new KeyNotFoundException($"Product with Id {id} not found.");
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<ICollection<GetProductDTO>> GetAllProductsAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsNoTracking()
                .Select(product => new GetProductDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    SKU = product.SKU,
                    Quantity = product.Quantity,
                    Description = product.Description,
                    Created_On = product.Created_On,
                    CategoryName = product.Category.Name,
                    SupplierFullName = $"{product.Supplier.FirstName} {product.Supplier.LastName}",
                    SupplierId = product.SupplierId
                })
                .ToListAsync();

            if (!products.Any())
            {
                throw new KeyNotFoundException("No products found.");
            }

            return products;
        }

        public async Task<GetProductDTO> GetProductAsync(string sku)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.SKU == sku);

            if (product == null)
            {
                throw new KeyNotFoundException($"Product with Id {sku} not found.");
            }

            return new GetProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                SKU = product.SKU,
                Quantity = product.Quantity,
                Description = product.Description,
                Created_On = product.Created_On,
                CategoryId = product.Category.Id,
                CategoryName = product.Category.Name,
                SupplierFullName = $"{product.Supplier.FirstName} {product.Supplier.LastName}",
                SupplierId = product.SupplierId
            };
        }

        public async Task UpdateProductAsync(UpdateProductDTO updateProductDTO)
        {
            await  _requestHelperRepository.CheckSKU(updateProductDTO.SKU);

            var product = await _context.Products.FindAsync(updateProductDTO.Id) ?? throw new KeyNotFoundException($"Product with Id {updateProductDTO.Id} not found.");
            product.Name = updateProductDTO.Name;
            product.Price = updateProductDTO.Price;
            product.SKU = updateProductDTO.SKU;
            product.Quantity = updateProductDTO.Quantity;
            product.Description = updateProductDTO.Description;
            product.CategoryId = updateProductDTO.CategoryId;
            product.SupplierId = updateProductDTO.SupplierId;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
    }
}