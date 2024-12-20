﻿using BikeRental_System3.DTOs.Request;
using BikeRental_System3.Models;

namespace BikeRental_System3.IService
{
    public interface IRentalRequestService
    {
        Task<RentalRequest> PostRentalRequest(RentalRequestRequest rentalReqRequest);
        Task<List<RentalRequest>> GetRentalRequests(Status? status);
        Task<RentalRequest> GetRentalRequest(Guid id);
       Task <List<RentalRequest>>  GetRentalRequestbyNic(string nicNumber);
        Task<RentalRequest> UpdateRentalRequest(Guid id, RentalRequest rentalRequest);
        Task<RentalRequest> AcceptRentalRequest(Guid Id);
        Task<RentalRequest> DeclineRentalRequest(Guid Id);
        Task<string> DeleteRentalRequest(Guid id);
        Task<int> GetPendingRentalRequestsCountAsync();
        Task<string> GetMostPopularNicAsync();
        Task<int> GetAcceptedRequestCountAsync();
        Task<int> GetDeclinedRequestCountAsync();



    }
}
