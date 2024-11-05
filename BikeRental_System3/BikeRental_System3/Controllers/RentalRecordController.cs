using BikeRental_System3.IService;
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
    }
}
