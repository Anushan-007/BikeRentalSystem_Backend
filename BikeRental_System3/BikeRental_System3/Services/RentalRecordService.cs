using BikeRental_System3.DTOs.Request;
using BikeRental_System3.IRepository;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using BikeRental_System3.Repository;

namespace BikeRental_System3.Services
{
    public class RentalRecordService : IRentalRecordService
    {
        private readonly IRentalRecordRepository _rentalRecordRepository;
        private readonly IRentalRequestRepository _rentalRequestService;
        private readonly IBikeUnitRepository _bikeUnitRepository;

        public RentalRecordService(IRentalRecordRepository rentalRecordRepository, IRentalRequestRepository rentalRequestRepository, IBikeUnitRepository bikeUnitRepository)
        {
            _rentalRecordRepository = rentalRecordRepository;
            _rentalRequestService = rentalRequestRepository;
            _bikeUnitRepository = bikeUnitRepository;


        }


        public async Task<RentalRecord> PostRentalRecord(RentalRecordRequest rentalRecRequest)
        {
            var RentalRecord = new RentalRecord()
            {
                RentalRequestId = rentalRecRequest.RentalRequestId,
                RentalOut = DateTime.Now,
                BikeRegNo = rentalRecRequest.BikeRegNo,
            };
            
            var data = await _rentalRecordRepository.PostRentalRecord(RentalRecord);
            var getUnit = await _bikeUnitRepository.GetInventoryUnit(rentalRecRequest.BikeRegNo);
            getUnit.Availability = false;
            _bikeUnitRepository.PutInventoryUnit(getUnit);
            var getRequest = await _rentalRequestService.GetRentalRequest(rentalRecRequest.RentalRequestId);
            getRequest.Status = Status.OnRent;
            var updated = await _rentalRequestService.UpdateRentalRequest(getRequest);
            return data;
        }

        public async Task<List<RentalRecord>> GetRentalRecords(State? state)
        {
            if (state == State.Incompleted)
            {
                var data = await _rentalRecordRepository.GetIncompleteRentalRecords();
                return data;
            }
            else if (state == State.Completed)
            {
                var data = await _rentalRecordRepository.GetRentalRecords();
                return data;
            }
            else
            {
                throw new Exception("Invalid State Code");
            }

        }

        public async Task<RentalRecord> GetRentalRecord(Guid id)
        {
            var data = await _rentalRecordRepository.GetRentalRecord(id);
            return data;
        }

        public async Task<RentalRecord> UpdateRentalRecord(Guid id, RentalRecord rentalRecord)
        {
            var getRecord = await _rentalRecordRepository.GetRentalRecord(id);
            if (getRecord != null)
            {
                var data = await _rentalRecordRepository.UpdateRentalRecord(rentalRecord);
                return data;
            }
            else
            {
                throw new NotImplementedException();
            }

        }



    }
}
