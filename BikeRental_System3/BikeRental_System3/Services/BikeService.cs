using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.IRepository;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using Microsoft.EntityFrameworkCore;

namespace BikeRental_System3.Services
{
    public class BikeService : IBikeService
    {
        private readonly IBikeRepository _bikeRepository;

        public BikeService(IBikeRepository bikeRepository)
        {
            _bikeRepository = bikeRepository;
        }

        public async Task<BikeResponse> AddBike(BikeRequest bikeRequest)
        {
            var bikes = new Bike
            {
                Brand = bikeRequest.Brand,
                Type = bikeRequest.Type,
                Model = bikeRequest.Model,
                RatePerHour = bikeRequest.RatePerHour
            };

            var data = await _bikeRepository.AddBike(bikes);
            var res = new BikeResponse
            {
                Id = data.Id,
                Brand = data.Brand,
                Type = data.Type,
                Model = data.Model,
                RatePerHour = data.RatePerHour

            };
            return res;
        }

        public async Task<List<BikeResponse>> GetAllBikes()
        {
            var data = await _bikeRepository.GetAllBikes();
            var list = data.Select(x => new BikeResponse
            {
                Id=x.Id,
                Brand = x.Brand,
                Type = x.Type,
                Model = x.Model,
                RatePerHour = x.RatePerHour
            }).ToList();
            return list;
        }

        public async Task<BikeResponse> GetBikeById(int Id)
        {
                var data = await _bikeRepository.GetBikeById(Id);
                if (data == null)
                {
                    throw new NotFoundException($"Bike with ID {Id} was not found.");
                }

            var res = new BikeResponse
                {
                    Id = data.Id,
                    Brand = data.Brand,
                    Type = data.Type,
                    Model = data.Model,
                    RatePerHour = data.RatePerHour
                };
                return res;
        }

        public async Task<BikeResponse> UpdateBike(int Id, BikeRequest bikeRequest)
        {
            var get = await _bikeRepository.GetBikeById(Id);
            get.Brand = bikeRequest.Brand;
            get.Type = bikeRequest.Type;
            get.Model = bikeRequest.Model;
            get.RatePerHour = bikeRequest.RatePerHour;

            if (get == null)
            {
                throw new NotFoundException($"Bike with ID {Id} was not found.");
            }

            var data = await _bikeRepository.UpdateBike(get);

            var res = new BikeResponse
            {
                Brand = data.Brand,
                Type = data.Type,
                Model = data.Model,
                RatePerHour = data.RatePerHour
            };
            return res;
        }

        public async Task<string> DeleteBike(int Id)
        {
            var get = await _bikeRepository.GetBikeById(Id);
            if (get == null)
            {
                throw new NotFoundException($"Bike with ID {Id} was not found.");
            }

            var data = await _bikeRepository.DeleteBike(get);
            return "Successfully Deleted";
        }

        public class NotFoundException : Exception
        {
            public NotFoundException(string message) : base(message) { }
        }

    }
}
