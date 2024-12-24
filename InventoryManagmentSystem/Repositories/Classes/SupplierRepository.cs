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
    public class SupplierRepository : ISupplierRepository
    {
        private readonly InventoryManagmentContext _context;

        public SupplierRepository(InventoryManagmentContext context)
        {
            _context = context;
        }

        public async Task AddSupplierAsync(AddSupplierDTO supplierDto)
        {
            if (supplierDto == null)
            {
                throw new ArgumentNullException(nameof(supplierDto), "Supplier data cannot be null.");
            }

            Supplier newSupplier = new Supplier
            {
                FirstName = supplierDto.FirstName,
                LastName = supplierDto.LastName,
                Email = supplierDto.Email,
                Phone = supplierDto.Phone,
                Address = supplierDto.Address
            };

            await _context.Suppliers.AddAsync(newSupplier);
            await _context.SaveChangesAsync();
        }

        public async Task<ICollection<GetSupplierDTO>> GetAllSuppliersAsync()
        {
            var suppliers = await _context.Suppliers
                .AsNoTracking()
                .Select(supplier => new GetSupplierDTO
                {
                    Id = supplier.Id,
                    FirstName = supplier.FirstName,
                    LastName = supplier.LastName,
                    Email = supplier.Email,
                    Phone = supplier.Phone,
                    Address = supplier.Address
                })
                .ToListAsync();

            if (!suppliers.Any())
            {
                throw new KeyNotFoundException("No suppliers found.");
            }

            return suppliers;
        }

        public async Task<GetSupplierDTO> GetSupplierAsync(int id)
        {
            var supplier = await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
            {
                throw new KeyNotFoundException($"Supplier with Id {id} not found.");
            }

            return new GetSupplierDTO
            {
                Id = supplier.Id,
                FirstName = supplier.FirstName,
                LastName = supplier.LastName,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address
            };
        }
    }
}