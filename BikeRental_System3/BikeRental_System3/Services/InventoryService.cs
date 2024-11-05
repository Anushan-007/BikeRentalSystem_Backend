using BikeRental_System3.IRepository;
using BikeRental_System3.IService;

namespace BikeRental_System3.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }
    }
}
