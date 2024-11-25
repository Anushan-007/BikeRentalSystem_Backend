﻿using BikeRental_System3.DTOs.Request;
using BikeRental_System3.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            try
            {
                var data = await _rentalRequestService.PostRentalRequest(rentalReqRequest);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}
