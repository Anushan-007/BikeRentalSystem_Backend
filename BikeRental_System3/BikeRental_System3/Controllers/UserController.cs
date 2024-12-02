using BikeRental_System3.DTOs.Request;
using BikeRental_System3.IService;
using BikeRental_System3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BikeRental_System3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService )
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
            var data = User.FindFirst("roles").Value;

            return data;
        }


        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GettAllUsers()
        {
            try
            {
                var data = await _userService.GetAllUsers();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetUserById")]
        public async Task<IActionResult> GetUserById(string NicNumber)
        {
            try
            {
                var data = await _userService.GetUserById(NicNumber);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(string NicNumber, UserRequest userRequest)
        {
            
                var data = await _userService.UpdateUser(NicNumber, userRequest);
                return Ok(data);
            
          
        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string NicNumber)
        {
            try
            {
                var data = await _userService.DeleteUser(NicNumber);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
