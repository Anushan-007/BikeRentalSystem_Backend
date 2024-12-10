using BikeRental_System3.Models;

namespace BikeRental_System3.IRepository
{
    public interface IRentalRecordRepository
    {

        Task<RentalRecord> PostRentalRecord(RentalRecord RentalRecord);
        Task<List<RentalRecord>> GetRentalRecords();
        Task<List<RentalRecord>> GetIncompleteRentalRecords();
        Task<RentalRecord> GetRentalRecord(Guid id);
        Task<RentalRecord> UpdateRentalRecord(RentalRecord RentalRecord);
        Task<RentalRecord> GetRentalRecordByReqId(Guid ReqId);
    }
}
