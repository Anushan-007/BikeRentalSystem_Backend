using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.IRepository;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using BikeRental_System3.Repository;
using Microsoft.EntityFrameworkCore;
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
        private readonly IRentalRequestRepository _rentalRequestRepository;
        private readonly IRentalRecordRepository _rentalRecordRepository;

        public UserService(IUserRepository userRepository, IConfiguration configuration, IRentalRequestRepository rentalRequestRepository, IRentalRecordRepository rentalRecordRepository)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _rentalRequestRepository = rentalRequestRepository;
            _rentalRecordRepository = rentalRecordRepository;
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
                roles = (Roles)userRequest.roles,
                UserName = userRequest.UserName,
                ProfileImage = userRequest.ProfileImage,
                IsBlocked = false,
            };

            if (userRequest.roles == Roles.Admin)
            {
                users.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin3");
                users.UserName = "Admin3";
            }

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


        public async Task<List<UserResponse>> GetAllUsers()
        {
            var data = await _userRepository.GetAllUsers();
            var list = data.Select(x => new UserResponse
            {
                NicNumber=x.NicNumber,
                FirstName=x.FirstName,
                LastName=x.LastName,
                Email=x.Email,
                ContactNo=x.ContactNo,
                Address=x.Address,
                roles=x.roles,
                UserName=x.UserName,
                ProfileImage=x.ProfileImage,
            }).ToList();
            return list;
        }

        //public async Task<UserResponse> GetUserById(string NicNumber)
        //{
        //    var data = await _userRepository.GetUserById(NicNumber);
        //    if (data == null)
        //    {
        //        throw new NotFoundException($"User with Nic Number {NicNumber} was not found.");
        //    }

        //    var res = new UserResponse
        //    {
        //        NicNumber = data.NicNumber,
        //        FirstName = data.FirstName,
        //        LastName = data.LastName,
        //        Email = data.Email,
        //        ContactNo = data.ContactNo,
        //        Address = data.Address,
        //        roles = data.roles,
        //        UserName = data.UserName,
        //        ProfileImage = data.ProfileImage,




        //    };


        //    return res;
        //}

        public async Task<UserResponse> GetUserById(string NicNumber)
        {
            var data = await _userRepository.GetUserById(NicNumber);
            if (data == null)
            {
                throw new NotFoundException($"User with Nic Number {NicNumber} was not found.");
            }

            // Fetch rental requests associated with this user
            var rentalRequests = await _rentalRequestRepository.GetRentalRequestbyNic(NicNumber);


            var res = new UserResponse
            {
                NicNumber = data.NicNumber,
                FirstName = data.FirstName,
                LastName = data.LastName,
                Email = data.Email,
                ContactNo = data.ContactNo,
                Address = data.Address,
                roles = data.roles,
                UserName = data.UserName,
                ProfileImage = data.ProfileImage,

                // Assign the rental requests to the response object
                RentalRequests = rentalRequests.Select(x => new RentalRequestResponse
                {
                    RentalRequestId = x.Id,
                    RequestTime = x.RequestTime,
                    Status = x.Status,
                    BikeId = x.BikeId,
                    NicNumber = x.NicNumber,
                }).ToList()

            };
            var responseList = new List<RentalRecordResponse>();    
            foreach (var item in res.RentalRequests.Where(r=> r.Status == Status.OnRent))
            {
                var response = new RentalRecordResponse();
                var getRecord = await _rentalRecordRepository.GetRentalRecordByReqId(item.RentalRequestId);
                response.RecordId = getRecord.Id;
                response.RentalRequestId = getRecord.RentalRequestId;
                response.RentalReturn = getRecord.RentalReturn;
                response.RentalOut = getRecord.RentalOut;
                response.Payment = getRecord.Payment;
                response.RentalReturn = getRecord.RentalReturn;
                responseList.Add(response);
            }
            res.RentalRecords = responseList;
            return res;
        }

        //public async Task<UserResponse> GetUserById(string NicNumber)
        //{
        //    // Fetch user data by NicNumber
        //    var data = await _userRepository.GetUserById(NicNumber);
        //    if (data == null)
        //    {
        //        throw new NotFoundException($"User with Nic Number {NicNumber} was not found.");
        //    }

        //    // Fetch rental requests associated with the user
        //    var rentalRequests = await _rentalRequestRepository.GetRentalRequestbyNic(NicNumber);

        //    // Create a response object to map the user details
        //    var res = new UserResponse
        //    {
        //        NicNumber = data.NicNumber,
        //        FirstName = data.FirstName,
        //        LastName = data.LastName,
        //        Email = data.Email,
        //        ContactNo = data.ContactNo,
        //        Address = data.Address,
        //        roles = data.roles,
        //        UserName = data.UserName,
        //        ProfileImage = data.ProfileImage,

        //        // Use Task.WhenAll to fetch all rental records asynchronously
        //        RentalRequests = await Task.WhenAll(rentalRequests.Select(async x =>
        //        {
        //            // Get rental records asynchronously for each rental request
        //            var rentalRecords = await _rentalRecordRepository.GetRentalRecordByReqId(x.Id);

        //            return new RentalRequestResponse
        //            {
        //                RentalRequestId = x.Id,
        //                RequestTime = x.RequestTime,
        //                Status = x.Status,
        //                BikeId = x.BikeId,
        //                NicNumber = x.NicNumber,


        //                RentalRecords = rentalRecords
        //            };
        //        }))
        //    };

        //    return res;
        //}




        //    RentalRequests = data.RentalRequest?.Select(r => new RentalRequestResponse
        //            {
        //                //Id = r.Id,
        //                RequestTime = r.RequestTime,
        //                Status = r.Status,
        //                BikeId = r.BikeId,
        //                UserAlert = r.UserAlert
        //}).ToList()


        public async Task<UserResponse> UpdateUser(String NicNumber, UserRequest userRequest)
        {
            var get = await _userRepository.GetUserById(NicNumber);
            get.FirstName = userRequest.FirstName;
            get.LastName = userRequest.LastName;
            get.Email = userRequest.Email;
            get.ContactNo = userRequest.ContactNo;
            get.Address = userRequest.Address;
            get.PasswordHash = userRequest.Password?? get.PasswordHash;
            get.UserName = userRequest.UserName;
            get.ProfileImage = userRequest.ProfileImage?? get.ProfileImage;

            if (get == null)
            {
                throw new NotFoundException($"Bike with ID {NicNumber} was not found.");
            }

            var data = await _userRepository.UpdateUser(get);

            var res = new UserResponse
            {
                FirstName = data.FirstName,
                LastName = data.LastName,
                Email = data.Email,
                ContactNo = data.ContactNo,
                Address = data.Address,
                UserName = data.UserName,
                ProfileImage = data.ProfileImage,

            };
            return res;
        }


        public async Task<string> DeleteUser(string NicNumber)
        {
            var get = await _userRepository.GetUserById(NicNumber);
            if (get == null)
            {
                throw new NotFoundException($"User with NIC Number {NicNumber} was not found.");
            }

            var data = await _userRepository.DeleteUser(get);
            return "Successfully Deleted";
        }



        public class NotFoundException : Exception
        {
            public NotFoundException(string message) : base(message) { }
        }

    }
}
