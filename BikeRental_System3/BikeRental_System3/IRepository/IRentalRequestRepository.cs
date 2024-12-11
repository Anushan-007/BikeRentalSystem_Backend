using BikeRental_System3.Models;

namespace BikeRental_System3.IRepository
{
    public interface IRentalRequestRepository
    {
        Task<RentalRequest> PostRentalRequest(RentalRequest rentalRequest);
        Task<List<RentalRequest>> GetRentalRequests();
        Task<List<RentalRequest>> GetRentalRequestsByStatus(Status? status);
        Task<RentalRequest> GetRentalRequest(Guid id);
        Task<List<RentalRequest>> GetRentalRequestbyNic(string nic);
        Task<RentalRequest> UpdateRentalRequest(RentalRequest rentalRequest);
        Task<string> DeleteRentalRequest(Guid id);
        Task<int> GetPendingRentalRequestsCountAsync();
         Task<string> GetMostPopularNicAsync();
        Task<int> GetAcceptedRequestCountAsync();
        Task<int> GetDeclinedRequestCountAsync();
    }
}
