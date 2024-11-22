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
                        ImagePath = img.ImagePath.Replace("wwwroot\\", "").Replace("\\", "//")

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



        //public async Task<BikeResponse> UpdateBike(BikeRequest bikeRequest)
        //{
        //    if (bikeRequest.BikeUnits == null || bikeRequest.BikeUnits.Count == 0)
        //    {
        //        throw new ArgumentException("No bike units provided.");
        //    }

        //    var bike = new Bike
        //    {
        //        //Id = bikeRequest.Id,
        //        Brand = bikeRequest.Brand,
        //        Type = bikeRequest.Type,
        //        Model = bikeRequest.Model,
        //    };

        //    // Update the bike details
        //    var updatedBike = await _bikeRepository.UpdateBike(bike);

        //    var updatedBikeUnits = new List<BikeUnit>();

        //    // Update bike units
        //    foreach (var bikeUnitRequest in bikeRequest.BikeUnits)
        //    {
        //        var unit = new BikeUnit
        //        {
        //            //UnitId = bikeUnitRequest.UnitId,
        //            BikeId = updatedBike.Id,
        //            RegistrationNumber = bikeUnitRequest.RegistrationNumber,
        //            Year = bikeUnitRequest.Year,
        //            RentPerDay = bikeUnitRequest.RentPerDay,
        //        };

        //        await _bikeRepository.UpdateBikeUnit(unit);
        //        updatedBikeUnits.Add(unit);
        //    }

        //    // Update images if present
        //    if (bikeRequest.Images != null && bikeRequest.Images.Count > 0)
        //    {
        //        var images = bikeRequest.Images.Select(imageRequest => new Image
        //        {
        //            BikeUnitId = bikeRequest.BikeUnits.FirstOrDefault()?.UnitId ?? Guid.Empty,
        //            ImagePath = imageRequest.ImagePath,
        //            ImageType = imageRequest.ImageType
        //        }).ToList();

        //        await _bikeRepository.UpdateBikeImages(images, bikeRequest.BikeUnits.FirstOrDefault()?.UnitId ?? Guid.Empty);
        //    }

        //    // Prepare response
        //    var res = new BikeResponse
        //    {
        //        Id = updatedBike.Id,
        //        Brand = updatedBike.Brand,
        //        Type = updatedBike.Type,
        //        Model = updatedBike.Model,
        //        BikeUnits = updatedBikeUnits.Select(p => new BikeUnitResponse
        //        {
        //            UnitId = p.UnitId,
        //            RegistrationNumber = p.RegistrationNumber,
        //            Year = p.Year,
        //            RentPerDay = p.RentPerDay
        //        }).ToList()
        //    };

        //    return res;
        //}


        //public async Task<BikeResponse> UpdateBike(BikeRequest bikeRequest)
        //{
        //    if (bikeRequest.BikeUnits == null || bikeRequest.BikeUnits.Count == 0)
        //    {
        //        throw new ArgumentException("No bike units provided.");
        //    }

        //    // Map BikeRequest to Bike entity
        //    var bike = new Bike
        //    {
        //        //Id = bikeRequest.Id, // Assuming you are passing bike Id in the request to update an existing bike
        //        Brand = bikeRequest.Brand,
        //        Type = bikeRequest.Type,
        //        Model = bikeRequest.Model,
        //    };

        //    var bikeUnits = new List<BikeUnit>();

        //    // Process each bike unit request
        //    foreach (var unitRequest in bikeRequest.BikeUnits)
        //    {
        //        var unit = new BikeUnit
        //        {
        //            //UnitId = unitRequest.UnitId,
        //            //BikeId = bikeRequest.Id, // Set the BikeId from the request
        //            RegistrationNumber = unitRequest.RegistrationNumber,
        //            Year = unitRequest.Year,
        //            RentPerDay = unitRequest.RentPerDay,
        //        };

        //        bikeUnits.Add(unit);
        //    }

        //    // Add or update the bike and bike units in the repository
        //    var updatedBike = await _bikeRepository.UpdateBike(bike);

        //    // Handle images if any
        //    if (bikeRequest.Images != null && bikeRequest.Images.Count > 0)
        //    {
        //        var images = bikeRequest.Images.Select(imageRequest => new Image
        //        {
        //            UnitId = bikeRequest.BikeUnits.FirstOrDefault()?.BikeId ?? Guid.Empty,
        //            ImagePath = imageRequest.ImagePath // Assuming ImagePath is a string
        //        }).ToList();

        //        await _bikeRepository.UpdateBikeImages(images, bikeRequest.BikeUnits.FirstOrDefault()?.BikeId ?? Guid.Empty);
        //    }

        //    // Prepare the response DTO
        //    var res = new BikeResponse
        //    {
        //        Id = updatedBike.Id,
        //        Brand = updatedBike.Brand,
        //        Type = updatedBike.Type,
        //        Model = updatedBike.Model,
        //        BikeUnits = updatedBike.BikeUnits.Select(bu => new BikeUnitResponse
        //        {
        //            UnitId = bu.UnitId,
        //            RegistrationNumber = bu.RegistrationNumber,
        //            Year = bu.Year,
        //            RentPerDay = bu.RentPerDay,
        //            Images = bu.Images.Select(i => new ImageResponse
        //            {
        //                Id = i.Id,
        //                ImagePath = i.ImagePath
        //            }).ToList()
        //        }).ToList()
        //    };

        //    return res;
        //}



        public async Task<bool> UpdateBikeUnit (BikeUnitUpdateDTO bikeUnitUpdateDTO)
        {
            var bike = await _bikeRepository.GetByRegNo(bikeUnitUpdateDTO.RegistrationNumber);
            if(bike == null)
            {
                throw new Exception("Bike unit not found");
            }

            var bikeUnit = bike.BikeUnits.FirstOrDefault(bu => bu.UnitId == bikeUnitUpdateDTO.UnitId);

            if(bikeUnit == null)
            {
                throw new Exception("Bike Unit not found");
            }

            bikeUnit.RegistrationNumber = bikeUnitUpdateDTO.RegistrationNumber;
            bikeUnit.Year = bikeUnitUpdateDTO.Year;
            bikeUnit.RentPerDay = bikeUnitUpdateDTO.RentPerDay;

            var unitUpdated = await _bikeRepository.UpadteUnit(bikeUnit);
            if (!unitUpdated)
            {
                throw new Exception("Failed to update bike unit");
            }

            if(bikeUnitUpdateDTO.BikeImages != null && bikeUnitUpdateDTO.BikeImages.Any())
            {
                var imageDirectory = Path.Combine("wwwroot", "bike_images");
                if(!Directory.Exists(imageDirectory))
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

                        using(var stream = new FileStream(filePath, FileMode.Create))
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













