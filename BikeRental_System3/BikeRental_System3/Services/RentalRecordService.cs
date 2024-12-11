using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.Enus;
using BikeRental_System3.IRepository;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using BikeRental_System3.Repository;
using Microsoft.EntityFrameworkCore;

namespace BikeRental_System3.Services
{
    public class RentalRecordService : IRentalRecordService
    {
        private readonly IRentalRecordRepository _rentalRecordRepository;
        private readonly IRentalRequestRepository _rentalRequestRepository;
        private readonly IBikeUnitRepository _bikeUnitRepository;
        private readonly sendmailService _sendmailService;
        private readonly IUserService _userService;
        public RentalRecordService(IRentalRecordRepository rentalRecordRepository, IRentalRequestRepository rentalRequestRepository, IBikeUnitRepository bikeUnitRepository, sendmailService sendmailService, IUserService userService)
        {
            _rentalRecordRepository = rentalRecordRepository;
            _rentalRequestRepository = rentalRequestRepository;
            _bikeUnitRepository = bikeUnitRepository;
            _sendmailService = sendmailService; 
            _userService = userService;

        }


        public async Task<RentalRecord> PostRentalRecord(RentalRecordRequest rentalRecRequest)
        {
            var RentalRecord = new RentalRecord()
            {
                RentalRequestId = rentalRecRequest.RentalRequestId,
                RentalOut = DateTime.Now,
                RegistrationNumber = rentalRecRequest.RegistrationNumber,
            };
            
            var data = await _rentalRecordRepository.PostRentalRecord(RentalRecord);
            var getUnit = await _bikeUnitRepository.GetInventoryUnit(rentalRecRequest.RegistrationNumber);
            getUnit.Availability = false;
            _bikeUnitRepository.PutInventoryUnit(getUnit);
            var getRequest = await _rentalRequestRepository.GetRentalRequest(rentalRecRequest.RentalRequestId);
            getRequest.Status = Status.OnRent;
            var updated = await _rentalRequestRepository.UpdateRentalRequest(getRequest);
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


        //public async Task<PaymentResponse> GetPayment(Guid id)
        //{
        //    var data = await _rentalRecordRepository.GetRentalRecord(id);
        //    var getRequest = await _rentalRequestRepository.GetRentalRequest(data.RentalRequestId);
        //    var getRate = getRequest.BikeUnit.RentPerDay;
        //    var timeSpan = DateTime.Now.Subtract((DateTime)data.RentalOut);
        //    var payment = getRate * (Decimal)timeSpan.TotalHours;

        //    var paymentResponse = new PaymentResponse
        //    {
        //        Payment = payment,
        //        RatePerHour = getRate,
        //    };
        //    return paymentResponse;
        //}


        //public async Task<PaymentResponse> GetPayment(Guid id)
        //{
        //    var data = await _rentalRecordRepository.GetRentalRecord(id);
        //    if (data == null) throw new Exception("Rental record not found.");

        //    var getRequest = await _rentalRequestRepository.GetRentalRequest(data.RentalRequestId);
        //    if (getRequest == null) throw new Exception("Rental request not found.");

        //    if (getRequest.BikeUnit == null) throw new Exception("Bike unit not found for rental request.");

        //    var getRate = getRequest.BikeUnit.RentPerDay;

        //    if (data.RentalOut == null) throw new Exception("Rental out time is not set.");

        //    var timeSpan = DateTime.Now.Subtract((DateTime)data.RentalOut);
        //    var payment = getRate * (Decimal)timeSpan.TotalHours;

        //    var paymentResponse = new PaymentResponse
        //    {
        //        Payment = payment,
        //        RatePerHour = getRate,
        //    };
        //    return paymentResponse;
        //}


        public async Task<PaymentResponse> GetPayment(Guid id)
        {
            // Retrieve the rental record
            var data = await _rentalRecordRepository.GetRentalRecord(id);
            if (data == null)
            {
                throw new Exception("Rental record not found.");
            }

            // Retrieve the rental request, including BikeUnit
            var getRequest = await _rentalRequestRepository.GetRentalRequest(data.RentalRequestId);
            if (getRequest == null)
            {
                throw new Exception("Rental request not found.");
            }

            // Check if BikeUnit is found
            if (getRequest.Bike == null)
            {
                throw new Exception($"Bike unit not found for rental request with ID: {getRequest.Id}");
            }

            var getRate = getRequest.Bike.RentPerHour;

            // Check if RentalOut is set
            if (data.RentalOut == null)
            {
                throw new Exception("Rental out time is not set.");
            }

            // Calculate payment
            var timeSpan = DateTime.Now.Subtract((DateTime)data.RentalOut);
            var payment = getRate * (Decimal)timeSpan.TotalHours;

            // Prepare payment response
            var paymentResponse = new PaymentResponse
            {
                Payment = payment,
                RatePerHour = getRate,
            };

            return paymentResponse;
        }



        //public async Task<List<RentalRecord>> GetOverDueRentals()
        //{
        //    var data = await _rentalRecordRepository.GetIncompleteRentalRecords();
        //    var overdue = new List<RentalRecord>();
        //    var now = DateTime.Now;
        //    foreach (RentalRecord record in data)
        //    {
        //        if (now.Subtract((DateTime)record.RentalOut).Hours > 24)
        //        {
        //            overdue.Add(record);
        //        }
        //    }
        //    return overdue;
        //}


        public async Task<List<RentalRecord>> GetOverDueRentalsOfUser(string? nicNo)
        {
            var data = await _rentalRecordRepository.GetIncompleteRentalRecords();
            var overdue = new List<RentalRecord>();
            var now = DateTime.Now;
            if (nicNo != null)
            {
                foreach (RentalRecord record in data)
                {
                    if (now.Subtract((DateTime)record.RentalOut).Minutes > 1 && record.RentalRequest.NicNumber == nicNo)
                    {
                        overdue.Add(record);
                    }

                }
            }
            else
            {
                foreach (RentalRecord record in data)
                {
                    if (now.Subtract((DateTime)record.RentalOut).Minutes > 1)
                    {
                        overdue.Add(record);
                    }

                }
            }

            return overdue;
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


        public async Task<RentalRecord> CompleteRentalRecord(Guid id, RentalRecordUpdateRequest rentalRecPutRequest)
        {
            var getRecord = await _rentalRecordRepository.GetRentalRecord(id);
            if (getRecord != null)
            {
                getRecord.RentalReturn = DateTime.Now;
                getRecord.Payment = rentalRecPutRequest.Payment;

               // var rentalRecRequest = new RentalRecordRequest();
                var getUnit = await _bikeUnitRepository.GetInventoryUnit(getRecord.RegistrationNumber);
                if (getUnit == null)
                {
                    throw new Exception("bike unit Not Found");
                }
                getUnit.Availability = true;
                await _bikeUnitRepository.PutInventoryUnit(getUnit);

                var data = await _rentalRecordRepository.UpdateRentalRecord(getRecord);
                var user = await _userService.GetUserById(getRecord.RentalRequest.NicNumber);
                var req = new SendMailRequest();
                req.RentalRecord = getRecord;
                req.RentalRequest = getRecord.RentalRequest;
                req.EmailType = EmailTypes.PaymentNotification;
                req.Email = user.Email;
                req.Name = "payment";
                var result = await _sendmailService.Sendmail(req);
                return data;
            }
            else
            {
                throw new NotImplementedException();
            }

        }

        //public async Task<List<RentalRecordResponse>> GetRentalRecordByReqId(Guid ReqId)
        //{
        //    var data = await _rentalRecordRepository.GetRentalRecordByReqId(ReqId);

        //    // Map the RentalRecord entities to RentalRecordResponse
        //    var rentalRecordResponses = data.Select(r => new RentalRecordResponse
        //    {
        //        RecordId = r.Id, // Assuming the property name is 'Id'
        //        RentalOut = r.RentalOut,
        //        RentalReturn = r.RentalReturn,
        //        Payment = r.Payment,
        //        RentalRequestId = r.RentalRequestId,
        //        RegistrationNumber = r.RegistrationNumber
        //    }).ToList();

        //    return rentalRecordResponses;
        //}


        public async Task<decimal> GetTotalPaymentAsync()
        {
            return await _rentalRecordRepository.GetTotalPaymentAsync();
        }


    }
}
