using Microsoft.EntityFrameworkCore;

namespace InventoryManagmentSystem.InventoryManagmentContext
{
    public class InventoryManagmentContext
    {
        public InventoryManagmentContext(DbContextOptions<InventoryManagmentContext> options) : base(options) { }
    }
}
