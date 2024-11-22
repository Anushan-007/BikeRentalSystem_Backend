using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.Models;
using Microsoft.AspNetCore.Mvc;

namespace BikeRental_System3.IService
{
    public interface IBikeService
    {
        Task<BikeResponse> AddBike(BikeRequest bikeRequest);
        Task<bool> AddBikeImages(ImageRequest imageRequest);

        Task<Bike> GetByRegNo(string RegNo);
        Task<List<BikeResponse>> GetAllBikesAsync();
        //Task<List<Bike>> AllBikes(int pagenumber, int pagesize);

        //Task<List<BikeResponse>> GetAllBikes();
        //Task<BikeResponse> GetBikeById(Guid Id);
        //Task<BikeResponse> UpdateBike(Guid Id, BikeRequest bikeRequest);
        // Task<BikeResponse> UpdateBike(BikeRequest bikeRequest);
        Task<bool> UpdateBikeUnit(BikeUnitUpdateDTO bikeUnitUpdateDTO);
        //Task<string> DeleteBike(Guid Id);
    }
}
