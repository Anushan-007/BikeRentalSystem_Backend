using BikeRental_System3.Data;
using BikeRental_System3.IRepository;
using BikeRental_System3.Models;

namespace BikeRental_System3.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> UserRegister(User user)
        {
            var data = await _context.AddAsync(user);
            await _context.SaveChangesAsync();
            return data.Entity;
        }


    }
}
