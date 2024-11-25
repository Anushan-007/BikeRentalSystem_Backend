using BikeRental_System3.DTOs.Request;
using BikeRental_System3.Models;

namespace BikeRental_System3.IService
{
    public interface IRentalRequestService
    {
        Task<RentalRequest> PostRentalRequest(RentalRequestRequest rentalReqRequest);
    }
}
