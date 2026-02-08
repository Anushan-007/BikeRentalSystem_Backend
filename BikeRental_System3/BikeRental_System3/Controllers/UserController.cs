using BikeRental_System3.DTOs.Request;
using BikeRental_System3.IService;
using BikeRental_System3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BikeRental_System3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("userRegister")]
        public async Task<IActionResult> UserRegister(UserRequest userRequest)
        {
            try
            {
                var data = await _userService.UserRegister(userRequest);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var loginResponse = await _userService.UserLogin(loginRequest);
                return Ok(loginResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("UserRoles")]
        public async Task<string> Test()
        {
            var data = User.FindFirst("roles")?.Value ?? "No role found";
            return data;
        }

        // Admin only endpoint - Get all users
        [Authorize]
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GettAllUsers()
        {
            try
            {
                // Check if user has admin role
                var userRole = User.FindFirst("roles")?.Value;
                if (userRole != "Admin")
                {
                    return StatusCode(403, new { message = "Access denied. Admin role required." });
                }

                var data = await _userService.GetAllUsers();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("GetUserById")]
        public async Task<IActionResult> GetUserById(string NicNumber)
        {
            try
            {
                // Allow users to get their own data, admins can get any user's data
                var currentUserNic = User.FindFirst("NicNumber")?.Value;
                var userRole = User.FindFirst("roles")?.Value;

                if (userRole != "Admin" && currentUserNic != NicNumber)
                {
                    return StatusCode(403, new { message = "Access denied. You can only access your own data." });
                }

                var data = await _userService.GetUserById(NicNumber);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(string NicNumber, UserRequest userRequest)
        {
            try
            {
                // Allow users to update their own data, admins can update any user's data
                var currentUserNic = User.FindFirst("NicNumber")?.Value;
                var userRole = User.FindFirst("roles")?.Value;

                if (userRole != "Admin" && currentUserNic != NicNumber)
                {
                    return StatusCode(403, new { message = "Access denied. You can only update your own data." });
                }

                var data = await _userService.UpdateUser(NicNumber, userRequest);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Admin only endpoint - Delete user
        [Authorize]
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string NicNumber)
        {
            try
            {
                // Only admins can delete users
                var userRole = User.FindFirst("roles")?.Value;
                if (userRole != "Admin")
                {
                    return StatusCode(403, new { message = "Access denied. Admin role required." });
                }

                var data = await _userService.DeleteUser(NicNumber);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Admin only endpoint - Block/Unblock user
        [Authorize]
        [HttpPut("blockUser")]
        public async Task<IActionResult> BlockUser(string NicNumber)
        {
            try
            {
                // Only admins can block users
                var userRole = User.FindFirst("roles")?.Value;
                if (userRole != "Admin")
                {
                    return StatusCode(403, new { message = "Access denied. Admin role required." });
                }

                var data = await _userService.BlockUser(NicNumber);
                return Ok(new { IsBlocked = data, Message = data ? "User blocked successfully" : "User unblocked successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // New endpoint to get current user's profile
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            try
            {
                var currentUserNic = User.FindFirst("NicNumber")?.Value;
                if (string.IsNullOrEmpty(currentUserNic))
                {
                    return BadRequest("Unable to identify user from token.");
                }

                var data = await _userService.GetUserById(currentUserNic);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
