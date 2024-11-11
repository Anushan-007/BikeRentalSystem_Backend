using BikeRental_System3.DTOs.Request;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
            try
            {
                var data = await _bikeService.AddBike(bikeRequest);
                return Ok(data);
            }catch (Exception ex)
            {
               return BadRequest(ex.Message);
            }
           
        }

        [HttpGet("GetAllBikes")]
        public async Task<IActionResult> GettAllBikes()
        {
            try
            {
                var data = await _bikeService.GetAllBikes();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetBikeById")]
        public async Task<IActionResult> GetBikeById(Guid Id)
        {
            try
            {
                var data = await _bikeService.GetBikeById(Id);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }

        [HttpPut("UpdateBike")]
        public async Task<IActionResult> UpdateBike(Guid Id, BikeRequest bikeRequest)
        {
            try
            {
                var data = await _bikeService.UpdateBike(Id, bikeRequest);
                return Ok(data);
            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteBike")]
        public async Task<IActionResult> DeleteBike(Guid Id)
        {
            try
            {
                var data = await _bikeService.DeleteBike(Id);
                return Ok(data);
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
