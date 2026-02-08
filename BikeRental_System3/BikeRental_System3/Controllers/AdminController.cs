using BikeRental_System3.Attributes;
using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeRental_System3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [RequireRole(Roles.Admin)]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRentalRequestService _rentalRequestService;

        public AdminController(IUserService userService, IRentalRequestService rentalRequestService)
        {
            _userService = userService;
            _rentalRequestService = rentalRequestService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            try
            {
                var allUsers = await _userService.GetAllUsers();
                var totalUsers = allUsers.Count;
                var blockedUsers = allUsers.Count(u => u.IsBlocked == true);
                var adminUsers = allUsers.Count(u => u.roles == Roles.Admin);
                var managerUsers = allUsers.Count(u => u.roles == Roles.Manager);
                var regularUsers = allUsers.Count(u => u.roles == Roles.User);

                var dashboardData = new
                {
                    TotalUsers = totalUsers,
                    BlockedUsers = blockedUsers,
                    AdminUsers = adminUsers,
                    ManagerUsers = managerUsers,
                    RegularUsers = regularUsers,
                    RecentUsers = allUsers.OrderByDescending(u => u.FirstName).Take(5)
                };

                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdminUser(AdminCreateRequest adminRequest)
        {
            try
            {
                var userRequest = new UserRequest
                {
                    NicNumber = adminRequest.NicNumber,
                    FirstName = adminRequest.FirstName,
                    LastName = adminRequest.LastName,
                    Email = adminRequest.Email,
                    ContactNo = adminRequest.ContactNo,
                    Address = adminRequest.Address,
                    UserName = adminRequest.UserName,
                    Password = adminRequest.Password,
                    roles = Roles.Admin
                };

                var result = await _userService.UserRegister(userRequest);
                return Ok(new { Message = "Admin user created successfully", Token = result.Token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("users/search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string searchTerm = "")
        {
            try
            {
                var allUsers = await _userService.GetAllUsers();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    allUsers = allUsers.Where(u =>
                    u.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
             u.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                   u.NicNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
              ).ToList();
                }

                return Ok(allUsers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("users/{nicNumber}/role")]
        public async Task<IActionResult> ChangeUserRole(string nicNumber, [FromBody] ChangeRoleRequest request)
        {
            try
            {
                var result = await _userService.ChangeUserRole(nicNumber, request.NewRole);
                return Ok(new { Message = $"User role changed successfully to {request.NewRole}", User = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("system-logs")]
        public async Task<IActionResult> GetSystemLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                // This is a placeholder for system logs functionality
                // You would implement actual logging mechanism
                var logs = new
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalLogs = 0,
                    Logs = new List<object>()
                };

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}