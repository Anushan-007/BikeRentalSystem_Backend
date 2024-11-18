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


        public async Task<User> GetUserByUsername(string username)
        {
            var data = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username); // Change 'Username' if using 'Email'
            return data;
        }



        public async Task<List<User>> GetAllUsers()
        {
            var data = await _context.Users.ToListAsync();
            return data;
        }

        public async Task<User> GetUserById(string NicNumber)
        {
            var data = await _context.Users.FirstOrDefaultAsync(b => b.NicNumber == NicNumber);
            if (data == null)
            {
                throw new NotFoundException($"User with NIC number {NicNumber} was not found.");
            }
            return data;
        }


        public async Task<User> UpdateUser(User user)
        {
            var data = _context.Users.Update(user);
            await _context.SaveChangesAsync();
            if (data == null)
            {
                throw new NotFoundException($"User with NIC Number {user} was not found.");
            }

            return data.Entity;

        }

        public async Task<string> DeleteUser(User user)
        {
            var data = _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            if (data == null)
            {
                throw new NotFoundException($"User with NIC Number {user} was not found.");

            }

            return "Successfully Deleted";
        }



        public class NotFoundException : Exception
        {
            public NotFoundException(string message) : base(message) { }
        }
    }
}
