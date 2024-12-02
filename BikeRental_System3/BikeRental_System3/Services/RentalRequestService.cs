using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
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
        private readonly IBikeUnitRepository _unitRepository;

        public RentalRequestService(IRentalRequestRepository repository , IBikeUnitRepository  unitRepository)
        {
            _repository = repository;
            _unitRepository = unitRepository;
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
               var data  = await _repository.GetRentalRequestsByStatus(status);
                foreach (var rentalRequest in data) {
                    var units = new List<BikeUnit>();
                    var getBike = rentalRequest.Bike;
                    
                    units = await _unitRepository.GetInventoryUnits(false , getBike.Id);
                    getBike.BikeUnits = units;
                   
                }
                return data;
            }

        }

        public async Task<RentalRequest> GetRentalRequest(Guid id)
        {
            var data = await _repository.GetRentalRequest(id);
            return data;
        }


        public async Task <List<RentalRequest>>  GetRentalRequestbyNic(string nicNumber)
        {
            var data = await _repository.GetRentalRequestbyNic(nicNumber);
            return data;
        }

        public async Task<RentalRequest> UpdateRentalRequest(Guid id, RentalRequest rentalRequest)
        {
            var getRequest = await _repository.GetRentalRequest(id);
            if (getRequest != null)
            {
                var data = await _repository.UpdateRentalRequest(rentalRequest);
                return data;
            }
            else
            {
                throw new NotImplementedException();
            }

        }

        public async Task<RentalRequest> AcceptRentalRequest(Guid Id)
        {
            var request = await _repository.GetRentalRequest(Id);
            request.Status = Status.Accepted;
            var data = await _repository.UpdateRentalRequest(request);
            return data;
        }
        public async Task<RentalRequest> DeclineRentalRequest(Guid Id)
        {
            var request = await _repository.GetRentalRequest(Id);
            request.Status = Status.Declined;
            var data = await _repository.UpdateRentalRequest(request);
            return data;
        }


        public async Task<string> DeleteRentalRequest(Guid id)
        {
            var data = await _repository.DeleteRentalRequest(id);
            return data;
        }

    }
}
