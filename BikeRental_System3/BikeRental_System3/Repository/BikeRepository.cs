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

        public async Task<Bike> AddBike(Bike bike)
        {
            var data = await _context.AddAsync(bike);
            await _context.SaveChangesAsync();
            return data.Entity;
        }

        public async Task<List<Bike>> GetAllBikes()
        {
            var data = await _context.Bikes.ToListAsync();
            return data;
        }

        public async Task<Bike> GetBikeById(Guid Id)
        {
            var data = await _context.Bikes.FirstOrDefaultAsync(b => b.Id == Id);
            if (data == null)
            {
                throw new NotFoundException($"Bike with ID {Id} was not found.");
            }
            return data;
        }

        public async Task<Bike> UpdateBike(Bike bike)
        {
            var data = _context.Bikes.Update(bike);
            await _context.SaveChangesAsync();
            if (data == null)
            {
                throw new NotFoundException($"Bike with ID {bike} was not found.");
            }
           
            return data.Entity;

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
