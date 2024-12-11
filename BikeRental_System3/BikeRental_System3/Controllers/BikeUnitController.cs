using BikeRental_System3.IService;
using BikeRental_System3.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BikeRental_System3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BikeUnitController : ControllerBase
    {
        private IBikeUnitService _bikeUnitService;

        public BikeUnitController(IBikeUnitService bikeUnitService)
        {
            _bikeUnitService = bikeUnitService;
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryUnit(bool? availability, Guid? bikeId)
        {
            try
            {
                var data = await _bikeUnitService.GetInventoryUnits(availability, bikeId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("availablityUnits")]
        public async Task<IActionResult> GetAvailablityUnits(bool? availability)
        {
            try
            {
                var data = await _bikeUnitService.GetAvailablityUnits(availability);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBikeUnit(string regNo)
        {
            try
            {
                var data = await _bikeUnitService.DeleteBikeUnit(regNo);
                return Ok(data);
            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("totalBikes")]
        public async Task<IActionResult> TotalBikesCount()
        {
            var data = await _bikeUnitService.TotalBikesCount();
            return Ok(data);
        }


        [HttpGet("available/count")]
        public async Task<IActionResult> GetAvailableBikeUnitsCount()
        {
            var count = await _bikeUnitService.GetAvailableBikeUnitsCountAsync();
            return Ok(count);
        }

        [HttpGet("count-unavailable")]
        public async Task<IActionResult> GetUnavailableBikeUnitsCount()
        {
            var count = await _bikeUnitService.GetUnavailableBikeUnitsCountAsync();
            return Ok(count);
        }

    }
}
