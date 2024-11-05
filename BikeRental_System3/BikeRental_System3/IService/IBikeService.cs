using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;

namespace BikeRental_System3.IService
{
    public interface IBikeService
    {
        Task<BikeResponse> AddBike(BikeRequest bikeRequest);
    }
}
