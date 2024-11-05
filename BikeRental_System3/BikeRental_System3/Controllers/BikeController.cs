using BikeRental_System3.DTOs.Request;
using BikeRental_System3.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BikeRental_System3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BikeController : ControllerBase
    {
        private readonly IBikeService _bikeService;

        public BikeController(IBikeService bikeService)
        {
            _bikeService = bikeService;
        }

        [HttpPost("BikeAdd")]
        public async Task<IActionResult> AddBike(BikeRequest bikeRequest)
        {
            var data = await _bikeService.AddBike(bikeRequest);
            return Ok(data);
        }

    }
}
