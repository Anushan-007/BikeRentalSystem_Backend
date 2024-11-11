using BikeRental_System3.DTOs.Request;
using BikeRental_System3.Models;

namespace BikeRental_System3.IRepository
{
    public interface IUserRepository
    {
        Task<User> UserRegister(User user);
        //Task<User> UserLogin(LoginRequest loginRequest);
        //Task<User> UserLogin(User user);

        Task<User> GetUserByUsername(string username);
    }
}
