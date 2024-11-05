using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.IRepository;
using BikeRental_System3.IService;
using BikeRental_System3.Models;

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

    }
}
