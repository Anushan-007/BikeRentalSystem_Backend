using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.Models;

namespace BikeRental_System3.IService
{
    public interface IUserService
    {
        Task<LoginResponse> UserRegister(UserRequest userRequest);
        Task<LoginResponse> UserLogin(LoginRequest loginRequest);
        LoginResponse CreateToken(User user);
    }
}
