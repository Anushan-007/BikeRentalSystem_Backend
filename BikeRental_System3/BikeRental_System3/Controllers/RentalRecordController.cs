using BikeRental_System3.DTOs.Request;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using BikeRental_System3.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BikeRental_System3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalRecordController : ControllerBase
    {
        private readonly IRentalRecordService _recordService;

        public RentalRecordController(IRentalRecordService recordService)
        {
            _recordService = recordService;
        }

        [HttpPost]
        public async Task<IActionResult> PostRentalRecord(RentalRecordRequest rentalRecRequest)
        {
            var data = await _recordService.PostRentalRecord(rentalRecRequest);
            return Ok(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetRentalRecord(State? state)
        {
            try
            {
                var data = await _recordService.GetRentalRecords(state);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRentalRecord(Guid id)
        {
            try
            {
                var data = await _recordService.GetRentalRecord(id);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRentalRecord(Guid id, RentalRecord rentalRecord)
        {
            try
            {
                var data = await _recordService.UpdateRentalRecord(id, rentalRecord);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}
