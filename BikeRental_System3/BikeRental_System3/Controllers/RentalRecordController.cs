using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using BikeRental_System3.Services;
using Microsoft.AspNetCore.Authorization;
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


        [HttpGet("get-payment{id}")]
        public async Task<IActionResult> GetRentalRecordPayment(Guid id)
        {
            try
            {
                var data = await _recordService.GetPayment(id);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[HttpGet("Get-overdue")]
        //public async Task<IActionResult> GetOverDueRentals()
        //{
        //    try
        //    {
        //        var data = await _recordService.GetOverDueRentals();
        //        return Ok(data);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}


        [HttpGet("Get-overdue")]
        public async Task<IActionResult> GetOverDueRentalsOfUser(string? nicNo)
        {
            try
            {
                var data = await _recordService.GetOverDueRentalsOfUser(nicNo);
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


        [HttpPut("complete-record")]
        public async Task<IActionResult> CompleteRentalRecord(Guid id, RentalRecordUpdateRequest rentalRecPutRequest)
        {
            var data = await _recordService.CompleteRentalRecord(id, rentalRecPutRequest);
            return Ok(data);
        }



        [HttpGet("GetByRentalRequestId/{reqId}")]
        public async Task<ActionResult<List<RentalRecordResponse>>> GetRentalRecordByReqId(Guid reqId)
        {
            // Call the service to get rental records by the RentalRequestId
            var rentalRecords = await _recordService.GetRentalRecordByReqId(reqId);

            // If no records are found, return a NotFound response
            if (rentalRecords == null || rentalRecords.Count == 0)
            {
                return NotFound("No rental records found for this request ID.");
            }

            // Return the mapped response
            return Ok(rentalRecords);
        }

    }
}
