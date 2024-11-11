using BikeRental_System3.Data;
using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.IRepository;
using BikeRental_System3.Models;
using Microsoft.EntityFrameworkCore;

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

        //public async Task<User> UserLogin(LoginRequest loginRequest)
        //{
        //    var data = await _context.Users.FirstOrDefaultAsync(u => u.UserName == loginRequest.UserName);
        //    return data;
        //}

        public async Task<User> GetUserByUsername(string username)
        {
            var data = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username); // Change 'Username' if using 'Email'
            return data;
        }

    }
}
