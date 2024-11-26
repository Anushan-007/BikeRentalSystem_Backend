using BikeRental_System3.IRepository;
using BikeRental_System3.IService;
using BikeRental_System3.Models;

namespace BikeRental_System3.Services
{
    public class BikeUnitService : IBikeUnitService
    {
        private IBikeUnitRepository _repository;

        public BikeUnitService(IBikeUnitRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BikeUnit>> GetInventoryUnits(bool? availability, Guid? bikeId)
        {
            return await _repository.GetInventoryUnits(availability, bikeId);
        }

        public async Task<BikeUnit> GetInventoryUnit(string registrationNumber)
        {
            var data = await _repository.GetInventoryUnit(registrationNumber);


            return data;
        }


        public async Task<BikeUnit> PutInventoryUnit(BikeUnit inventoryUnit)
        {
            var data = await _repository.PutInventoryUnit(inventoryUnit);
            return inventoryUnit;
        }

    }
}
