using BikeRental_System3.Models;

namespace BikeRental_System3.IRepository
{
    public interface IBikeUnitRepository
    {
        Task<List<BikeUnit>> GetInventoryUnits(bool? availability, Guid? bikeId);
        Task<BikeUnit> GetInventoryUnit(string RegistrationNumber);
        Task<BikeUnit> PutInventoryUnit(BikeUnit inventoryUnit);
    }
}
