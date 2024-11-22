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
        //Task<BikeResponse> GetBikeById(Guid Id);
        Task<BikeResponse> GetBikeDetailsAsync(Guid bikeId);
        //Task<bool> UpdateBikeUnit(BikeUnitUpdateDTO bikeUnitUpdateDTO);
        Task<bool> UpdateBikeAndUnitsAndImages(Guid bikeId, BikeUnitUpdateDTO bikeUnitUpdateDTO);
       
        //Task<string> DeleteBike(Guid Id);
    }
}
