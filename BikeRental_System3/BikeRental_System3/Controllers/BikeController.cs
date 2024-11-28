using BikeRental_System3.DTOs.Request;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using BikeRental_System3.Services;
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
        public async Task<IActionResult> AddBike([FromBody] BikeRequest bikeRequest)
        {
            // Log the incoming bikeRequest object
            if (bikeRequest == null)
            {
                return BadRequest("No bike request data received.");
            }

            if (bikeRequest.BikeUnits == null || bikeRequest.BikeUnits.Count == 0)
            {
                return BadRequest("No bike units provided.");
            }

            try
            {
                var data = await _bikeService.AddBike(bikeRequest);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }


        [HttpPost("AddBikeImages")]
        public async Task<IActionResult> AddBikeImages([FromForm] ImageRequest imageRequest)
        {
            var data = await _bikeService.AddBikeImages(imageRequest);
            return Ok(data);
        }


        //[HttpGet("GetAllBikes")]
        //public async Task<IActionResult> GetAllBikes()
        //{
        //    try
        //    {
        //        var bikes = await _bikeService.GetAllBikesAsync();
        //        return Ok(bikes);
        //    }
        //    catch(Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpGet("GetAllBikes")]
        public async Task<IActionResult> GetAllBikes()
        {
            try
            {
                var bikes = await _bikeService.GetAllBikesAsync();
                return Ok(bikes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




        [HttpGet("GetBikeById")]
        public async Task<IActionResult> GetBikeById(String RegNo)
        {
            try
            {
                var data = await _bikeService.GetByRegNo(RegNo);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        [HttpGet("{bikeId}")]
        public async Task<IActionResult> GetBikeDetails(Guid bikeId)
        {
            // Call the service to get the bike details
            var bikeDetails = await _bikeService.GetBikeDetailsAsync(bikeId);

            if (bikeDetails == null)
            {
                return NotFound(); // Return 404 if bike not found
            }

            return Ok(bikeDetails); // Return 200 with the BikeResponse DTO
        }


        //[HttpPut("UpdateBike")]
        //public async Task<IActionResult> UpdateBikeUnit([FromForm] BikeUnitUpdateDTO bikeUnitUpdateDTO)
        //{
        //    try
        //    {
        //        var data = await _bikeService.UpdateBikeUnit(bikeUnitUpdateDTO);
        //        return Ok(data);
        //    }catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpPut("{bikeId}/UpdateBike")]
        public async Task<IActionResult> UpdateBikeUnit(Guid bikeId, [FromForm] BikeUnitUpdateDTO bikeUnitUpdateDTO)
        {
            try
            {
                // Call the service to update the bike, its units, and images
                var result = await _bikeService.UpdateBikeAndUnitsAndImages(bikeId, bikeUnitUpdateDTO);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        //[HttpDelete("DeleteBike")]
        //public async Task<IActionResult> DeleteBike(Guid Id)
        //{
        //    try
        //    {
        //        var data = await _bikeService.DeleteBike(Id);
        //        return Ok(data);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

    }
}
