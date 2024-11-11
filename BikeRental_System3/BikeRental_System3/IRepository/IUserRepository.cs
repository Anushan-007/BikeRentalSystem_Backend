using BikeRental_System3.Models;

namespace BikeRental_System3.IRepository
{
    public interface IUserRepository
    {
        Task<User> UserRegister(User user);
    }
}
