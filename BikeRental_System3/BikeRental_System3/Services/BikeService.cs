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
                        ImagePath = img.ImagePath.Replace("wwwroot\\", "").Replace("\\", "//")

                    }).ToList() ?? new List<ImageResponse>()
                }).ToList()
            }).ToList();

            return bikeDtos;
        }




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


        public async Task<BikeResponse> GetBikeDetailsAsync(Guid bikeId)
        {
            // Fetch the Bike from the repository
            var bike = await _bikeRepository.GetBikeByIdAsync(bikeId);

            if (bike == null)
            {
                return null; // Or throw an exception if the bike is not found
            }

            // Manually map the Bike entity to the BikeResponse DTO
            var bikeResponse = new BikeResponse
            {
                Id = bike.Id,
                Brand = bike.Brand,
                Type = bike.Type,
                Model = bike.Model,
                BikeUnits = bike.BikeUnits.Select(bu => new BikeUnitResponse
                {
                    UnitId = bu.UnitId,
                    RegistrationNumber = bu.RegistrationNumber,
                    Year = bu.Year,
                    RentPerDay = bu.RentPerDay,
                    Images = bu.Images?.Select(i => new ImageResponse
                    {
                        Id = i.Id,
                        ImagePath = i.ImagePath
                    }).ToList()
                }).ToList()
            };

            return bikeResponse;
        }

        //public async Task<bool> UpdateBikeUnit (BikeUnitUpdateDTO bikeUnitUpdateDTO)
        //{
        //    var bike = await _bikeRepository.GetByRegNo(bikeUnitUpdateDTO.RegistrationNumber);
        //    if(bike == null)
        //    {
        //        throw new Exception("Bike unit not found");
        //    }

        //    var bikeUnit = bike.BikeUnits.FirstOrDefault(bu => bu.UnitId == bikeUnitUpdateDTO.UnitId);

        //    if(bikeUnit == null)
        //    {
        //        throw new Exception("Bike Unit not found");
        //    }

        //    bikeUnit.RegistrationNumber = bikeUnitUpdateDTO.RegistrationNumber;
        //    bikeUnit.Year = bikeUnitUpdateDTO.Year;
        //    bikeUnit.RentPerDay = bikeUnitUpdateDTO.RentPerDay;

        //    var unitUpdated = await _bikeRepository.UpadteUnit(bikeUnit);
        //    if (!unitUpdated)
        //    {
        //        throw new Exception("Failed to update bike unit");
        //    }

        //    if(bikeUnitUpdateDTO.BikeImages != null && bikeUnitUpdateDTO.BikeImages.Any())
        //    {
        //        var imageDirectory = Path.Combine("wwwroot", "bike_images");
        //        if(!Directory.Exists(imageDirectory))
        //        {
        //            Directory.CreateDirectory(imageDirectory);
        //        }

        //        var bikeImages = new List<Image>();
        //        foreach (var bikeImage in bikeUnitUpdateDTO.BikeImages)
        //        {
        //            if (bikeImage != null && bikeImage.Length > 0)
        //            {
        //                var uniqueFileName = $"{Guid.NewGuid()}_{bikeImage.FileName}";
        //                var filePath = Path.Combine(imageDirectory, uniqueFileName);

        //                using(var stream = new FileStream(filePath, FileMode.Create))
        //                {
        //                    await bikeImage.CopyToAsync(stream);
        //                }

        //                bikeImages.Add(new Image
        //                {
        //                    UnitId = bikeUnitUpdateDTO.UnitId,
        //                    ImagePath = filePath,
        //                });
        //            }
        //        }

        //        var imageUpdated = await _bikeRepository.UpdateBikeImages(bikeImages);
        //        if (!imageUpdated)
        //        {
        //            throw new Exception("Failed to update bike images");

        //        }
        //    }
        //    return true;
        //}


        public async Task<bool> UpdateBikeAndUnitsAndImages(Guid bikeId, BikeUnitUpdateDTO bikeUnitUpdateDTO)
        {
            // Retrieve the bike by ID
            var bike = await _bikeRepository.GetBikeByIdAsync(bikeId);

            if (bike == null)
            {
                throw new Exception("Bike not found");
            }

            // Update the bike details (Brand, Type, Model, etc.)
            bike.Brand = bikeUnitUpdateDTO.Brand;
            bike.Type = bikeUnitUpdateDTO.Type;
            bike.Model = bikeUnitUpdateDTO.Model;

            // Loop through each BikeUnit to update the unit details
            foreach (var bikeUnit in bike.BikeUnits)
            {
                if (bikeUnit.UnitId == bikeUnitUpdateDTO.UnitId)
                {
                    // Update the BikeUnit details from the DTO
                    bikeUnit.RegistrationNumber = bikeUnitUpdateDTO.RegistrationNumber;
                    bikeUnit.Year = bikeUnitUpdateDTO.Year;
                    bikeUnit.RentPerDay = bikeUnitUpdateDTO.RentPerDay;

                    // Update the bike unit in the repository
                    var unitUpdated = await _bikeRepository.UpadteUnit(bikeUnit);
                    if (!unitUpdated)
                    {
                        throw new Exception("Failed to update bike unit");
                    }
                }
            }

            // Handle updating images for the bike unit
            if (bikeUnitUpdateDTO.BikeImages != null && bikeUnitUpdateDTO.BikeImages.Any())
            {
                var imageDirectory = Path.Combine("wwwroot", "bike_images");
                if (!Directory.Exists(imageDirectory))
                {
                    Directory.CreateDirectory(imageDirectory);
                }

                var bikeImages = new List<Image>();
                foreach (var bikeImage in bikeUnitUpdateDTO.BikeImages)
                {
                    if (bikeImage != null && bikeImage.Length > 0)
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}_{bikeImage.FileName}";
                        var filePath = Path.Combine(imageDirectory, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await bikeImage.CopyToAsync(stream);
                        }

                        bikeImages.Add(new Image
                        {
                            UnitId = bikeUnitUpdateDTO.UnitId,
                            ImagePath = filePath,
                        });
                    }
                }

                var imageUpdated = await _bikeRepository.UpdateBikeImages(bikeImages);
                if (!imageUpdated)
                {
                    throw new Exception("Failed to update bike images");
                }
            }

            // Save the updated bike
            var bikeUpdated = await _bikeRepository.UpadteUnit(bike.BikeUnits.FirstOrDefault(bu => bu.UnitId == bikeUnitUpdateDTO.UnitId));
            if (!bikeUpdated)
            {
                throw new Exception("Failed to update the bike");
            }

            return true;
        }


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













