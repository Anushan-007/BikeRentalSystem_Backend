using BikeRental_System3.DTOs.Request;
using BikeRental_System3.IRepository;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using BikeRental_System3.Repository;
using Microsoft.VisualBasic;

namespace BikeRental_System3.Services
{
    public class RentalRequestService : IRentalRequestService
    { 
        private readonly IRentalRequestRepository _repository;

        public RentalRequestService(IRentalRequestRepository repository)
        {
            _repository = repository;
        }


        public async Task<RentalRequest> PostRentalRequest(RentalRequestRequest rentalReqRequest)
        {
            var now = DateTime.Now;
            if (rentalReqRequest.RequestTime.Subtract(now).Minutes < 0)
            {
                throw new Exception("Check your request Date");
            }
            var rentalRequest = new RentalRequest()
            {
                BikeId = rentalReqRequest.BikeId,
                RequestTime = rentalReqRequest.RequestTime,
                NicNumber = rentalReqRequest.NicNumber,
                Status = Status.Pending,
            };
            var data = await _repository.PostRentalRequest(rentalRequest);
            return data;
        }



        public async Task<List<RentalRequest>> GetRentalRequests(Status? status)
        {
            if (status == null)
            {
                return await _repository.GetRentalRequests();
            }
            else
            {
                return await _repository.GetRentalRequestsByStatus(status);
            }

        }
    }
}
