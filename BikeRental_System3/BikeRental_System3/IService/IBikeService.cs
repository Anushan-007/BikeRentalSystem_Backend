using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.Models;

namespace BikeRental_System3.IService
{
    public interface IBikeService
    {
        Task<BikeResponse> AddBike(BikeRequest bikeRequest);
        Task<List<BikeResponse>> GetAllBikes();
        Task<BikeResponse> GetBikeById(Guid Id);
        Task<BikeResponse> UpdateBike(Guid Id, BikeRequest bikeRequest);
        Task<string> DeleteBike(Guid Id);
    }
}
