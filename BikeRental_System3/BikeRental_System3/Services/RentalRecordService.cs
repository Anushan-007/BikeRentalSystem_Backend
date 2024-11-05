using BikeRental_System3.IRepository;
using BikeRental_System3.IService;

namespace BikeRental_System3.Services
{
    public class RentalRecordService : IRentalRecordService
    {
        private readonly IRentalRecordRepository _rentalRecordRepository;

        public RentalRecordService(IRentalRecordRepository rentalRecordRepository)
        {
            _rentalRecordRepository = rentalRecordRepository;
        }
    }
}
