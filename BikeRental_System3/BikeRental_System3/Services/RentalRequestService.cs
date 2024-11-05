using BikeRental_System3.IRepository;
using BikeRental_System3.IService;

namespace BikeRental_System3.Services
{
    public class RentalRequestService : IRentalRequestService
    { 
        private readonly IRentalRequestRepository _repository;

        public RentalRequestService(IRentalRequestRepository repository)
        {
            _repository = repository;
        }
    }
}
