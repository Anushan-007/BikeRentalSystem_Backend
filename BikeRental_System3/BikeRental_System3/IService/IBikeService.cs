﻿using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.Models;
using Microsoft.AspNetCore.Mvc;

namespace BikeRental_System3.IService
{
    public interface IBikeService
    {
        Task<BikeResponse> AddBike(BikeRequest bikeRequest);
        Task<Bike> GetByRegNo(string RegNo);
        //Task<List<BikeResponse>> GetAllBikes();
        //Task<BikeResponse> GetBikeById(Guid Id);
        //Task<BikeResponse> UpdateBike(Guid Id, BikeRequest bikeRequest);
        //Task<string> DeleteBike(Guid Id);
        Task<bool> AddBikeImages(ImageRequest imageRequest);
    }
}
