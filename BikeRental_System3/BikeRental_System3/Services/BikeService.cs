using Azure.Core;
using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.IRepository;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using BikeRental_System3.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;

namespace BikeRental_System3.Services
{
    public class BikeService : IBikeService
    {
        private readonly IBikeRepository _bikeRepository;
        private readonly string _imageFolder;

        public BikeService(IBikeRepository bikeRepository)
        {
            _bikeRepository = bikeRepository;
            _imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");



            // Ensure the folder exists
            if (!Directory.Exists(_imageFolder))
            {
                Directory.CreateDirectory(_imageFolder);
            }
        }

        public async Task<BikeResponse> AddBike(BikeRequest bikeRequest)
        {
            if (bikeRequest.BikeUnits == null || bikeRequest.BikeUnits.Count == 0)
            {
                throw new ArgumentException("No bike units provided.");
            }

            var bike = new Bike
            {
                Brand = bikeRequest.Brand,
                Type = bikeRequest.Type,
                Model = bikeRequest.Model,
            };
            var addBike = _bikeRepository.AddBike(bike);

            var bikeUnits = new List<BikeUnit>();

            // Loop through each bike unit in the request
            foreach (var bikeUnitRequest in bikeRequest.BikeUnits)
            {

                // Create a new BikeUnit object
                var unit = new BikeUnit
                {
                    BikeId = addBike.Result,
                    RegistrationNumber = bikeUnitRequest.RegistrationNumber,
                    Year = bikeUnitRequest.Year,
                    RentPerDay = bikeUnitRequest.RentPerDay,
                    //Images = images
                };

                bikeUnits.Add(unit);

             await _bikeRepository.AddBikeUnit(unit);

                // Iterate over each image in the Images list for the current BikeUnit

            }

            var res = new BikeResponse
            {
                Id = addBike.Result,
                Brand = bike.Brand,
                Type = bike.Type,
                Model = bike.Model,
                BikeUnits = bikeUnits.Select(p => new BikeUnitResponse
                {
                    UnitId = p.UnitId,
                    RegistrationNumber = p.RegistrationNumber,
                    Year = p.Year,
                    RentPerDay = p.RentPerDay
                }).ToList()
            };
            return res;
        }
          
        public async Task<bool> AddBikeImages(ImageRequest imageRequest)
        {
            var bikeImages = new List<Image>();


            var imageDirectory = Path.Combine("wwwroot", "bike_images");
            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{imageRequest.ImagePath.FileName}";
            var filePath = Path.Combine(imageDirectory, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageRequest.ImagePath.CopyToAsync(stream);
            }


            var image = new Image
            {
                UnitId = imageRequest.UnitId,
                ImagePath = filePath
            };
            await _bikeRepository.AddBikeImages(image);
            return true;
        }

        public async Task<Bike> GetByRegNo(string RegNo)
        {
            var data = await _bikeRepository.GetByRegNo(RegNo);

            if (data == null)
            {
                throw new Exception("No Such Bike!");
            }
            return data;
        }


        //public async Task<List<BikeResponse>> GetAllBikes()
        //{
        //    var data = await _bikeRepository.GetAllBikes();
        //    foreach (var bike in data)
        //    {
        //        bike.Image = bike.Image?.Replace("wwwroot", "");
        //        bike.Image = bike.Image?.Replace("\\", "/");
        //    }
        //    var list = data.Select(x => new BikeResponse
        //    {
        //        Id = x.Id,
        //        Brand = x.Brand,
        //        Type = x.Type,
        //        Model = x.Model,
        //        RatePerHour = x.RatePerHour,
        //        Image = x.Image
        //    }).ToList();
        //    return list;
        //}

        //public async Task<List<Bike>> GetAllBikesAsync()
        //{
        //    return await _bikeRepository.GetAllBikes();
        //}


        public async Task<List<BikeResponse>> GetAllBikesAsync()
        {
            var bikes = await _bikeRepository.GetAllBikes();

            // Log the bikes and their images to debug
            foreach (var bike in bikes)
            {
                foreach (var unit in bike.BikeUnits)
                {
                    Console.WriteLine($"BikeUnit {unit.UnitId} Images: {unit.Images?.Count ?? 0}");
                }
            }

            // Continue mapping as usual
            var bikeDtos = bikes.Select(b => new BikeResponse
            {
                Id = b.Id,
                Brand = b.Brand,
                Type = b.Type,
                Model = b.Model,
                BikeUnits = b.BikeUnits.Select(bu => new BikeUnitResponse
                {
                    UnitId = bu.UnitId,
                    RegistrationNumber = bu.RegistrationNumber,
                    Year = bu.Year,
                    RentPerDay = bu.RentPerDay,
                    Images = bu.Images?.Select(img => new ImageResponse
                    {
                        Id = img.Id,
                        ImagePath = img.ImagePath.Replace("wwwroot\\","").Replace("\\","//")
                       
                    }).ToList() ?? new List<ImageResponse>()
                }).ToList()
            }).ToList();

            return bikeDtos;
        }



        //public async Task<List<Bike>> AllBikes(int pagenumber, int pagesize)
        //{
        //    var data = await _bikeRepository.AllBikes(pagenumber, pagesize);

        //    if (data == null)
        //    {
        //        throw new Exception("Data Not Found!");

        //    }
        //    return data;
        //}

        //public async Task<BikeResponse> GetBikeById(Guid Id)
        //    {
        //        var data = await _bikeRepository.GetBikeById(Id);
        //        if (data == null)
        //        {
        //            throw new NotFoundException($"Bike with ID {Id} was not found.");
        //        }

        //        var res = new BikeResponse
        //        {
        //            Id = data.Id,
        //            Brand = data.Brand,
        //            Type = data.Type,
        //            Model = data.Model,
        //            RatePerHour = data.RatePerHour,
        //            Image = data.Image
        //        };
        //        return res;
        //    }

        //    public async Task<BikeResponse> UpdateBike(Guid Id, BikeRequest bikeRequest)
        //    {
        //        var get = await _bikeRepository.GetBikeById(Id);
        //        get.Brand = bikeRequest.Brand;
        //        get.Type = bikeRequest.Type;
        //        get.Model = bikeRequest.Model;
        //        get.RatePerHour = bikeRequest.RatePerHour;
        //        //get.Image = bikeRequest?.Image;

        //        if (get == null)
        //        {
        //            throw new NotFoundException($"Bike with ID {Id} was not found.");
        //        }

        //        var data = await _bikeRepository.UpdateBike(get);

        //        var res = new BikeResponse
        //        {
        //            Brand = data.Brand,
        //            Type = data.Type,
        //            Model = data.Model,
        //            RatePerHour = data.RatePerHour
        //        };
        //        return res;
        //    }

        //    public async Task<string> DeleteBike(Guid Id)
        //    {
        //        var get = await _bikeRepository.GetBikeById(Id);
        //        if (get == null)
        //        {
        //            throw new NotFoundException($"Bike with ID {Id} was not found.");
        //        }

        //        var data = await _bikeRepository.DeleteBike(get);
        //        return "Successfully Deleted";
        //    }

        public class NotFoundException : Exception
        {
            public NotFoundException(string message) : base(message) { }
        }


     }

 }













