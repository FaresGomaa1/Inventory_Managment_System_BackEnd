using InventoryManagmentSystem.Data;
using InventoryManagmentSystem.Repositories.Interfaces;

namespace InventoryManagmentSystem.Repositories.Classes
{
    public class RequestRepository: IRequestRepository
    {
        private readonly InventoryManagmentContext _context;

        // Inject the DbContext into the repository
        public RequestRepository(InventoryManagmentContext context)
        {
            _context = context;
        }

    }
}
