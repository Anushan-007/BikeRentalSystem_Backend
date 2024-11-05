using BikeRental_System3.Data;
using BikeRental_System3.IRepository;

namespace BikeRental_System3.Repository
{
    public class BikeRepository : IBikeRepository
    {
        private readonly AppDbContext _context;

        public BikeRepository(AppDbContext context)
        {
            _context = context;
        }
    }
}
