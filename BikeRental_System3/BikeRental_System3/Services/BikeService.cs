using Azure.Core;
using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.IRepository;
using BikeRental_System3.IService;
using BikeRental_System3.Migrations;
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

            var bikeUnits = new List<BikeUnit>();
            var images = new List<Image>();

            // Loop through each bike unit in the request
            foreach (var bikeUnitRequest in bikeRequest.BikeUnits)
            {
                if (bikeUnitRequest.Images == null || bikeUnitRequest.Images.Count == 0)
                {
                    throw new ArgumentException("No images uploaded for bike unit.");
                }

                foreach (var imageFile in bikeUnitRequest.Images)
                {
                    // Ensure that the file is not null or empty
                    if (imageFile.Length > 0)  // Length is a property of IFormFile, not Image
                    {
                        // Generate a unique file name to avoid conflicts
                        var fileName = Path.GetFileName(imageFile.FileName);  // FileName is a property of IFormFile
                        var filePath = Path.Combine(_imageFolder, fileName);   // Set the file path (can add a subfolder if needed)

                        // Save the image to disk
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);  // CopyToAsync is a method of IFormFile
                        }

                        // Create an Image entity to store in the database
                        var bikeImage = new Image
                        {
                          //  UnitId = unit.UnitId,  // Make sure to associate the image with the bike unit
                            ImagePath = filePath,  // Store the file path or URL
                        };

                        images.Add(bikeImage);  // Add the image to the images list
                    }
                }

                // Create a new BikeUnit object
                var unit = new BikeUnit
                {
                    BikeId = bikeUnitRequest.BikeId,
                    RegistrationNumber = bikeUnitRequest.RegistrationNumber,
                    Year = bikeUnitRequest.Year,
                    RentPerDay = bikeUnitRequest.RentPerDay,
                    Images = images
                };

                bikeUnits.Add(unit);

                // Iterate over each image in the Images list for the current BikeUnit
                
            }

            // Save the Bike to the database
            var savedBike = await _bikeRepository.AddBike(bike);

            // Save BikeUnits
            foreach (var unit in bikeUnits)
            {
                await _bikeRepository.AddBikeUnit(unit);
            }

            // Save Images (the ImagePath is now being saved in the database)
            foreach (var image in images)
            {
                await _bikeRepository.AddBikeImages(new List<Image> { image });
            }

            var res = new BikeResponse
            {
                Id = savedBike.Id,
                Brand = savedBike.Brand,
                Type = savedBike.Type,
                Model = savedBike.Model,
            };

            return res;
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














//    // Ensure the folder exists
//    if (!Directory.Exists(_imageFolder))
//    {
//        Directory.CreateDirectory(_imageFolder);
//    }

//}

//public async Task<BikeResponse> AddBike(BikeRequest bikeRequest)
//{


//    if (bikeRequest.BikeUnits.Images == null || bikeRequest.BikeUnits.Images.Length == 0)
//    {
//        throw new ArgumentException("No image uploaded.");
//    }

//    // Save the image to the wwwroot/images folder
//    var fileName = Path.GetFileName(bikeRequest.BikeUnits.Images.FileName);
//    var filePath = Path.Combine(_imageFolder, fileName);

//    using (var stream = new FileStream(filePath, FileMode.Create))
//    {
//        await bikeRequest.BikeUnits.Images.CopyToAsync(stream);
//    }

//    var bikeUnits = new List<BikeUnit>();

//    foreach (var bikeUnt in bikeRequest.BikeUnits)
//    {

//        var unit = new BikeUnit
//        {
//            BikeId = bikeUnt.BikeId,
//            RegistrationNumber = bikeUnt.RegistrationNumber,
//            Year = bikeUnt.Year,
//            RentPerDay = bikeUnt.RentPerDay
//        };

//        bikeUnits.Add(unit);

//        var bikeimg = new Image
//        {
//            UnitId = bikeUnt.UnitId,
//            Images = bikeUnt.Images,
//        };
//    }


//    var bikes = new Bike
//    {
//        Brand = bikeRequest.Brand,
//        Type = bikeRequest.Type,
//        Model = bikeRequest.Model,
//        //Image = fileName
//    };

//    var data = await _bikeRepository.AddBike(bikes);
//    var res = new BikeResponse
//    {
//        Id = data.Id,
//        Brand = data.Brand,
//        Type = data.Type,
//        Model = data.Model,

//    };
//    return res;

