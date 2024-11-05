using BikeRental_System3.IRepository;
using BikeRental_System3.IService;

namespace BikeRental_System3.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
    }
}
