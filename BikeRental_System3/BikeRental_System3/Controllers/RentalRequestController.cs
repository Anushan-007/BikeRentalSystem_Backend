﻿using BikeRental_System3.DTOs.Request;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BikeRental_System3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalRequestController : ControllerBase
    {
        private readonly IRentalRequestService _rentalRequestService;

        public RentalRequestController(IRentalRequestService rentalRequestService)
        {
            _rentalRequestService = rentalRequestService;
        }


        [HttpPost]
        public async Task<IActionResult> PostRentalRequest([FromBody] RentalRequestRequest rentalReqRequest)
        {
           
                var data = await _rentalRequestService.PostRentalRequest(rentalReqRequest);
                return Ok(data);
       

        }

        [HttpGet]
        public async Task<IActionResult> GetRentalRequest(Status? status)
        {          
            try
            {
                var data = await _rentalRequestService.GetRentalRequests(status);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetRentalRequest(Guid id)
        {
            var rentalRequest = await _rentalRequestService.GetRentalRequest(id);

            if (rentalRequest == null)
            {
                return NotFound();
            }

            return Ok(rentalRequest);
        }

        [HttpGet("user/{nicNumber}")]
        public async Task<IActionResult> GetRentalRequestbyNic(string nicNumber)
        {
            var data = await _rentalRequestService.GetRentalRequestbyNic(nicNumber);
            return Ok(data);
        }

        [HttpGet("Accept-Request{id}")]
        public async Task<IActionResult> AcceptRenatlRequest(Guid id)
        {
            var data = await _rentalRequestService.AcceptRentalRequest(id);
            return Ok(data);
        }
        [HttpGet("Decline-Request{id}")]
        public async Task<IActionResult> DeclineRenatlRequest(Guid id)
        {
            var data = await _rentalRequestService.DeclineRentalRequest(id);
            return Ok(data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRentalRequest(Guid id)
        {
            var data = await _rentalRequestService.DeleteRentalRequest(id);
            if (string.IsNullOrEmpty(data))
            {
                return BadRequest("Failed to delete rental request.");
            }

            return NoContent();
        }

        [HttpGet("pending-count")]
        public async Task<ActionResult<int>> GetPendingRentalRequestsCount()
        {
            var count = await _rentalRequestService.GetPendingRentalRequestsCountAsync();
            return Ok(count);
        }

        [HttpGet("most-popular-nic")]
        public async Task<ActionResult<string>> GetMostPopularNic()
        {
            var mostPopularNic = await _rentalRequestService.GetMostPopularNicAsync();

            if (string.IsNullOrEmpty(mostPopularNic))
            {
                return NotFound("No NIC number found.");
            }

            return Ok(mostPopularNic); // Return the most popular NIC number as a string
        }

        // GET api/rentalrequest/acceptedcount
        [HttpGet("acceptedcount")]
        public async Task<ActionResult<int>> GetAcceptedRequestCount()
        {
            var count = await _rentalRequestService.GetAcceptedRequestCountAsync();
            return Ok(count); // Returns the count of accepted rental requests
        }

        [HttpGet("declinedcount")]
        public async Task<ActionResult<int>> GetDeclinedRequestCountAsync()
        {
            var count = await _rentalRequestService.GetDeclinedRequestCountAsync();
            return Ok(count); // Returns the count of accepted rental requests
        }

    }
}
