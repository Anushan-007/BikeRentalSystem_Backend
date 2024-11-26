using BikeRental_System3.Models;

namespace BikeRental_System3.IService
{
    public interface IBikeUnitService
    {
        Task<List<BikeUnit>> GetInventoryUnits(bool? availability, Guid? bikeId);
        Task<BikeUnit> GetInventoryUnit(string registrationNumber);
    }
}
