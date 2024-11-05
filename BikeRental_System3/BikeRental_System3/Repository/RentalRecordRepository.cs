using BikeRental_System3.Data;
using BikeRental_System3.IRepository;

namespace BikeRental_System3.Repository
{
    public class RentalRecordRepository : IRentalRecordRepository
    {
        private readonly AppDbContext _context;

        public RentalRecordRepository(AppDbContext context)
        {
            _context = context;
        }
    }
}
