using BikeRental_System3.Data;
using BikeRental_System3.IRepository;

namespace BikeRental_System3.Repository
{
    public class RentalRequestRepository : IRentalRequestRepository
    {
        private readonly AppDbContext _context;

        public RentalRequestRepository(AppDbContext context)
        {
            _context = context;
        }
    }
}
