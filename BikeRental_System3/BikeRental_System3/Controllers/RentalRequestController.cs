using BikeRental_System3.DTOs.Request;
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

    }
}
