using BikeRental_System3.Models;

namespace BikeRental_System3.IRepository
{
    public interface IRentalRequestRepository
    {
        Task<RentalRequest> PostRentalRequest(RentalRequest rentalRequest);
        Task<List<RentalRequest>> GetRentalRequests();
        Task<List<RentalRequest>> GetRentalRequestsByStatus(Status? status);
    }
}
