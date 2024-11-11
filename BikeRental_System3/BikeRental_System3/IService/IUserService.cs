using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;

namespace BikeRental_System3.IService
{
    public interface IUserService
    {
        Task<UserResponse> UserRegister(UserRequest userRequest);
    }
}
