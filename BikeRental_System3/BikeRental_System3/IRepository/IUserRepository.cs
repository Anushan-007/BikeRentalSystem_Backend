using BikeRental_System3.DTOs.Request;
using BikeRental_System3.Models;

namespace BikeRental_System3.IRepository
{
    public interface IUserRepository
    {
        Task<User> UserRegister(User user);

        Task<User> GetUserByUsername(string username);
        Task<List<User>> GetAllUsers();
        Task<User> GetUserById(string NicNumber);
        Task<User> UpdateUser(User user);
        Task<string> DeleteUser(User user);
        Task<bool> BlockUser(User user);
    }
}
