using BikeRental_System3.Data;
using BikeRental_System3.IRepository;

namespace BikeRental_System3.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }


    }
}
