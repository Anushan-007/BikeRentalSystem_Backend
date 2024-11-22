using BikeRental_System3.Data;
using BikeRental_System3.IRepository;
using BikeRental_System3.Models;
using Microsoft.EntityFrameworkCore;

namespace BikeRental_System3.Repository
{
    public class BikeRepository : IBikeRepository
    {
        private readonly AppDbContext _context;

        public BikeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> AddBike(Bike bike)
        {
            
            var data = await _context.AddAsync(bike);
            await _context.SaveChangesAsync();
            Guid id= bike.Id;
            return id;
        }

        public async Task<Guid> AddBikeUnit(BikeUnit unit)

        {
            await _context.BikeUnits.AddAsync(unit);
            await _context.SaveChangesAsync();
            _context.Entry(unit).Reload();
            return unit.UnitId;
        }

        public async Task<bool> AddBikeImages(Image image )
        {
            await _context.Images.AddRangeAsync(image);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Bike> GetByRegNo(string RegNo)
        {
            var findbike = await _context.BikeUnits

                         .Include(bi => bi.Images)
                         .FirstOrDefaultAsync(b => b.RegistrationNumber == RegNo);

            if (findbike == null)
            {
                throw new Exception("Error: Bike not found");
            }


            var getbike = await _context.Bikes
                   .Include(b => b.BikeUnits.Where(bu => bu.UnitId == findbike.UnitId))
                   .ThenInclude(bu => bu.Images)
                   .FirstOrDefaultAsync(b => b.Id == findbike.BikeId);

            if (getbike != null)
            {
                return getbike;
            }
            else
            {
                throw new Exception("Invalid!");
            }

        }

        public async Task<List<Bike>> GetAllBikes()
        {
            var data = await _context.Bikes.Include(b => b.BikeUnits).ThenInclude(bi => bi.Images).ToListAsync();
            return data;
        }

        //public async Task<List<Bike>> AllBikes(int pagenumber, int pagesize)
        //{

        //    int skip = (pagenumber - 1) * pagesize;
        //    var data = await _context.Bikes
        //    .Include(b => b.BikeUnits)
        //    .ThenInclude(bi => bi.Images)
        //    .Skip(skip).Take(pagesize)
        //    .ToListAsync();


        //    return data;

        //}

        public async Task<Bike> GetBikeById(Guid Id)
        {
            var data = await _context.Bikes.FirstOrDefaultAsync(b => b.Id == Id);
            if (data == null)
            {
                throw new NotFoundException($"Bike with ID {Id} was not found.");
            }
            return data;
        }

        //public async Task<Bike> UpdateBike(Bike bike)
        //{
        //    var data = _context.Bikes.Update(bike);
        //    await _context.SaveChangesAsync();
        //    if (data == null)
        //    {
        //        throw new NotFoundException($"Bike with ID {bike} was not found.");
        //    }

        //    return data.Entity;

        //}

        //public async Task<Bike> UpdateBike(Bike bike)
        //{
        //    var existingBike = await _context.Bikes.FindAsync(bike.Id);
        //    if (existingBike == null)
        //    {
        //        throw new ArgumentException("Bike not found.");
        //    }

        //    existingBike.Brand = bike.Brand;
        //    existingBike.Type = bike.Type;
        //    existingBike.Model = bike.Model;

        //    _context.Bikes.Update(existingBike);
        //    await _context.SaveChangesAsync();

        //    return existingBike;
        //}




        //public async Task<BikeUnit> UpdateBikeUnit(BikeUnit unit)
        //{
        //    var existingUnit = await _context.BikeUnits.FindAsync(unit.UnitId);
        //    if (existingUnit == null)
        //    {
        //        throw new ArgumentException("Bike unit not found.");
        //    }

        //    existingUnit.RegistrationNumber = unit.RegistrationNumber;
        //    existingUnit.Year = unit.Year;
        //    existingUnit.RentPerDay = unit.RentPerDay;

        //    _context.BikeUnits.Update(existingUnit);
        //    await _context.SaveChangesAsync();

        //    return existingUnit;
        //}

        //public async Task<bool> UpdateBikeImages(List<Image> images, Guid bikeUnitId)
        //{

        //    var existingImages = await _context.Images.Where(i => i.UnitId == bikeUnitId).ToListAsync();
        //    _context.Images.RemoveRange(existingImages);


        //    foreach (var image in images)
        //    {
        //        _context.Images.Add(image);
        //    }

        //    await _context.SaveChangesAsync();
        //    return true;
        //}


        //public async Task<Bike> UpdateBike(Bike bike)
        //{
        //    var existingBike = await _context.Bikes.Include(b => b.BikeUnits).ThenInclude(bu => bu.Images).FirstOrDefaultAsync(b => b.Id == bike.Id);
        //    if (existingBike == null)
        //    {
        //        throw new ArgumentException("Bike not found.");
        //    }

        //    existingBike.Brand = bike.Brand;
        //    existingBike.Type = bike.Type;
        //    existingBike.Model = bike.Model;

        //    // Update related BikeUnits and Images
        //    foreach (var bikeUnit in bike.BikeUnits)
        //    {
        //        var existingUnit = existingBike.BikeUnits.FirstOrDefault(bu => bu.UnitId == bikeUnit.UnitId);
        //        if (existingUnit != null)
        //        {
        //            existingUnit.RegistrationNumber = bikeUnit.RegistrationNumber;
        //            existingUnit.Year = bikeUnit.Year;
        //            existingUnit.RentPerDay = bikeUnit.RentPerDay;

        //            // Update images
        //            if (bikeUnit.Images != null)
        //            {
        //                foreach (var image in bikeUnit.Images)
        //                {
        //                    var existingImage = existingUnit.Images.FirstOrDefault(i => i.Id == image.Id);
        //                    if (existingImage != null)
        //                    {
        //                        existingImage.ImagePath = image.ImagePath; // Assuming you only update the ImagePath.
        //                    }
        //                    else
        //                    {
        //                        existingUnit.Images.Add(image);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    _context.Bikes.Update(existingBike);
        //    await _context.SaveChangesAsync();

        //    return existingBike;
        //}


        //public async Task<bool> UpdateBikeImages(List<Image> images, Guid unitId)
        //{
        //    var unit = await _context.BikeUnits.Include(bu => bu.Images).FirstOrDefaultAsync(bu => bu.UnitId == unitId);
        //    if (unit == null)
        //    {
        //        throw new ArgumentException("Bike unit not found.");
        //    }

        //    // Remove old images
        //    _context.Images.RemoveRange(unit.Images);

        //    // Add new images
        //    foreach (var image in images)
        //    {
        //        unit.Images.Add(image);
        //    }

        //    await _context.SaveChangesAsync();
        //    return true;
        //}



        public async Task<bool> UpadteUnit(BikeUnit bikeUnit)
        {
            _context.BikeUnits.Update(bikeUnit);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateBikeImages (List<Image> bikeImages)
        {
            _context.Images.UpdateRange(bikeImages);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<string> DeleteBike(Bike bike)
        {
           var data = _context.Bikes.Remove(bike);
            await _context.SaveChangesAsync();
            if (data == null)
            {
                throw new NotFoundException($"Bike with ID {bike} was not found.");

            }
            
            return "Successfully Deleted";
        }

        public class NotFoundException : Exception
        {
            public NotFoundException(string message) : base(message) { }
        }


    }
}
