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

        //public async Task<List<Bike>> GetAllBikes()
        //{
        //    var data = await _context.Bikes.Include(b => b.BikeUnits).ThenInclude(bi => bi.Images).ToListAsync();
        //    return data;
        //}

        public async Task<List<Bike>> GetAllBikes()
        {
            var data = await _context.Bikes
                .Include(b => b.BikeUnits)
                    .ThenInclude(bi => bi.Images)
                .Where(b => b.BikeUnits.Any(bu => bu.Availability))  // Only bikes with at least one available unit
                .ToListAsync();
            return data;
        }

        public async Task<List<Bike>> AllBikes()
        {
            var data = await _context.Bikes
                .Include(b => b.BikeUnits)
                    .ThenInclude(bi => bi.Images)
                .ToListAsync();
            return data;
        }



        public async Task<Bike> GetBikeByIdAsync(Guid bikeId)
        {
            return await _context.Bikes.Include(b => b.BikeUnits).ThenInclude(bu => bu.Images).FirstOrDefaultAsync(b => b.Id == bikeId);
        }


        //public async Task<Bike> GetBikeById(Guid Id)
        //{
        //    var data = await _context.Bikes.FirstOrDefaultAsync(b => b.Id == Id);
        //    if (data == null)
        //    {
        //        throw new NotFoundException($"Bike with ID {Id} was not found.");
        //    }
        //    return data;
        //}

        public async Task<BikeUnit> GetUnitById(Guid unitId)
        {
            var data = await _context.BikeUnits.FirstOrDefaultAsync(x => x.UnitId == unitId);
            return data;
        }

      

        public async Task<bool> UpadteUnit(BikeUnit bikeUnit)
        {
            _context.BikeUnits.Update(bikeUnit);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateBikeImages (Guid UnitId, List<Image> bikeImages)
        {
            var findImage = await _context.Images.Where(x => x.BikeUnitId == UnitId).ToListAsync();
            if (findImage != null)
            {
                _context.Images.RemoveRange(findImage);
                _context.Images.UpdateRange(bikeImages);
            }
            
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateBike(Bike bike)
        {
            _context.Bikes.Update(bike);
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
