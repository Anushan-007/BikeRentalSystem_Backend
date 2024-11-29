using BikeRental_System3.DTOs.Response;
using BikeRental_System3.IRepository;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using BikeRental_System3.Repository;
using System.Numerics;

namespace BikeRental_System3.Services
{
    public class BikeUnitService : IBikeUnitService
    {
        private IBikeUnitRepository _repository;

        public BikeUnitService(IBikeUnitRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BikeUnit>> GetInventoryUnits(bool? availability, Guid? bikeId)
        {
            return await _repository.GetInventoryUnits(availability, bikeId);
        }

        public async Task<BikeUnit> GetInventoryUnit(string registrationNumber)
        {
            var data = await _repository.GetInventoryUnit(registrationNumber);


            return data;
        }


        public async Task<BikeUnit> PutInventoryUnit(BikeUnit inventoryUnit)
        {
            var data = await _repository.PutInventoryUnit(inventoryUnit);
            return inventoryUnit;
        }

        public async Task<List<BikeUnitResponse>> GetAvailablityUnits(bool? availability)
        {
            // Retrieve the list of BikeUnit entities from the repository
            var data = await _repository.GetAvailablityUnits(availability);

            // Map the list of BikeUnit entities to a list of BikeUnitResponse objects
            var response = data.Select(unit => new BikeUnitResponse
            {
                UnitId = unit.UnitId,
                BikeId = unit.BikeId,
                RegistrationNumber = unit.RegistrationNumber,
                Year = unit.Year,
                Availability = unit.Availability,
                // Map the Images to the ImageResponse model
                //Images = unit.Images?.Select(image => new ImageResponse
                //{
                    
                //    Id = image.Id,
                //    ImagePath = image.ImagePath
                //}).ToList()
            }).ToList();

            return response;
        }

        public async Task<Message> DeleteBikeUnit(string registrationNumber)
        {
            var get = await _repository.GetBikeUnitById(registrationNumber);
            if (get == null)
            {
                throw new NotFoundException($"User with NIC Number {registrationNumber} was not found.");
            }

            var data = await _repository.DeleteBikeUnit(get);
            var message = new Message
            {
                text = "Successfully Deleted"
            };
            return message;
        }

        public class NotFoundException : Exception
        {
            public NotFoundException(string message) : base(message) { }
        }

    }
}