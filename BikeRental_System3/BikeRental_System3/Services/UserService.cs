using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.IRepository;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using BikeRental_System3.Repository;

namespace BikeRental_System3.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserResponse> UserRegister(UserRequest userRequest)
        {
            var users = new User
            {
                NicNumber = userRequest.NicNumber,
                FirstName = userRequest.FirstName,
                LastName = userRequest.LastName,
                Email = userRequest.Email,
                ContactNo = userRequest.ContactNo,
                Address = userRequest.Address,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRequest.Password),
                AccountCreated = DateTime.Now,
                roles = userRequest.roles,
                UserName = userRequest.UserName,
                ProfileImage = userRequest.ProfileImage,
            };

            var data = await _userRepository.UserRegister(users);
            var res = new UserResponse
            {
                NicNumber = users.NicNumber,
                FirstName = users.FirstName,
                LastName = users.LastName,
                Email = users.Email,
                ContactNo = users.ContactNo,
                Address = users.Address,
                roles = users.roles,
                UserName = users.UserName,
                ProfileImage = users.ProfileImage

            };
            return res;
        }

    }
}
