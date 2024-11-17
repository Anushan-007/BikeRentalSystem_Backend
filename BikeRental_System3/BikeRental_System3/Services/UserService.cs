using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.IRepository;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using BikeRental_System3.Repository;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BikeRental_System3.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponse> UserRegister(UserRequest userRequest)
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
            var token = CreateToken(data);
            return token;
        }
       

        public async Task<LoginResponse> UserLogin(LoginRequest loginRequest)
        {
            var user = await _userRepository.GetUserByUsername(loginRequest.UserName);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Verify password using BCrypt
            var passwordMatch = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash);
            if (!passwordMatch)
            {
                throw new Exception("Invalid password");
            }

            // Create and return JWT token
            var token = CreateToken(user);

            return token;
        }



        public LoginResponse CreateToken(User user)
        {
            var claimList = new List<Claim>();
            claimList.Add(new Claim("NicNumber", user.NicNumber));
            claimList.Add(new Claim("FirstName", user.FirstName));
            claimList.Add(new Claim("LastName", user.LastName));
            claimList.Add(new Claim("Email", user.Email));
            claimList.Add(new Claim("ContactNo", user.ContactNo));
            claimList.Add(new Claim("Address", user.Address));
            claimList.Add(new Claim("Password", user.PasswordHash));
            claimList.Add(new Claim("AccountCreated", DateTime.Now.ToString()));
            claimList.Add(new Claim("roles", user.roles.ToString()));
            claimList.Add(new Claim("IsBlocked", user.IsBlocked.ToString()));
            claimList.Add(new Claim("UserName", user.UserName));
            //claimList.Add(new Claim("ProfileImage", user.ProfileImage.ToString()));

            var Key = _configuration["Jwt:Key"];
            var secKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
            var credentials = new SigningCredentials(secKey, SecurityAlgorithms.HmacSha256);

            var togen = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claimList,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: credentials
                );

            var res = new LoginResponse();
            res.Token = new JwtSecurityTokenHandler().WriteToken(togen);
            return res;

        }

    }
}
