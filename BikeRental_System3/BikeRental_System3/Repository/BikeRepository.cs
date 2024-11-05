using BikeRental_System3.Data;
using BikeRental_System3.IRepository;
using BikeRental_System3.Models;

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
            return bike;
        }

        

    }
}
