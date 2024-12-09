using BikeRental_System3.DTOs.Response;
using BikeRental_System3.Models;

namespace BikeRental_System3.IService
{
    public interface IBikeUnitService
    {
        Task<List<BikeUnit>> GetInventoryUnits(bool? availability, Guid? bikeId);
        Task<BikeUnit> GetInventoryUnit(string registrationNumber);
        Task<List<BikeUnitResponse>> GetAvailablityUnits(bool? availability);
        Task<Message> DeleteBikeUnit(string registrationNumber);
        Task<int> TotalBikesCount();
        Task<int> GetAvailableBikeUnitsCountAsync();
    }
}
