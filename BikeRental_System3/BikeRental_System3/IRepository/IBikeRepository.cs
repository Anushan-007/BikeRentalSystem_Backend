using BikeRental_System3.DTOs.Request;
using BikeRental_System3.Models;

namespace BikeRental_System3.IRepository
{
    public interface IBikeRepository
    {
         Task<Guid> AddBike(Bike bike);
        Task<Guid> AddBikeUnit(BikeUnit unit);
        Task<bool> AddBikeImages(Image imageRequest);
        Task<Bike> GetByRegNo(string RegNo);
        Task<List<Bike>> GetAllBikes();
        //Task<List<Bike>> GetAllBikes();
        //Task<Bike> GetBikeById(Guid Id);
        Task<List<Bike>> AllBikes();
        Task<Bike> GetBikeByIdAsync(Guid bikeId);
        Task<BikeUnit> GetUnitById(Guid unitId);
        Task<bool> UpadteUnit(BikeUnit bikeUnit);
        Task<bool> UpdateBikeImages(Guid UnitId, List<Image> bikeImages);
        Task<string> DeleteBike(Bike bike);

    }
}
