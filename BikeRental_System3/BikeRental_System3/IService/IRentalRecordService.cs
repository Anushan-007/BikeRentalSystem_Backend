using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.Models;

namespace BikeRental_System3.IService
{
    public interface IRentalRecordService
    {
        Task<RentalRecord> PostRentalRecord(RentalRecordRequest rentalRecRequest);
        Task<List<RentalRecord>> GetRentalRecords(State? state);
        Task<RentalRecord> GetRentalRecord(Guid id);
        Task<RentalRecord> UpdateRentalRecord(Guid id, RentalRecord rentalRecord);
        Task<PaymentResponse> GetPayment(Guid id);
        Task<RentalRecord> CompleteRentalRecord(Guid id, RentalRecordUpdateRequest rentalRecPutRequest);

    }
}
