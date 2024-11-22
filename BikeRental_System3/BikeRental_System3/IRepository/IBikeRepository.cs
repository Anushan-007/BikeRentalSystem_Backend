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
        //Task<List<Bike>> AllBikes(int pagenumber, int pagesize);
        Task<Bike> GetBikeById(Guid Id);
        //Task<Bike> UpdateBike(Bike bike);
        //Task<Bike> UpdateBike(Bike bike);
        //Task<BikeUnit> UpdateBikeUnit(BikeUnit unit);
        //Task<bool> UpdateBikeImages(List<Image> images, Guid bikeUnitId);
        //Task<Bike> UpdateBike(Bike bike);
        //Task<bool> UpdateBikeImages(List<Image> images, Guid unitId);
        Task<bool> UpadteUnit(BikeUnit bikeUnit);
        Task<bool> UpdateBikeImages(List<Image> bikeImages);
        Task<string> DeleteBike(Bike bike);

        


    }
}
