using BikeRental_System3.DTOs.Response;
using BikeRental_System3.Models;

namespace BikeRental_System3.IRepository
{
    public interface IBikeUnitRepository
    {
        Task<List<BikeUnit>> GetInventoryUnits(bool? availability, Guid? bikeId);
        Task<BikeUnit> GetInventoryUnit(string RegistrationNumber);
        Task<BikeUnit> PutInventoryUnit(BikeUnit inventoryUnit);
        Task<List<BikeUnit>> GetAvailablityUnits(bool? availability);
        Task<BikeUnit> GetBikeUnitById(string regNo);
        Task<Message> DeleteBikeUnit(BikeUnit bikeUnits);
        Task<int> TotalBikesCount();
        Task<int> GetAvailableBikeUnitsCountAsync();
        }
}
