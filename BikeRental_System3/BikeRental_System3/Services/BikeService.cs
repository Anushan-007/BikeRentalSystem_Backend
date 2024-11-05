using BikeRental_System3.IRepository;
using BikeRental_System3.IService;

namespace BikeRental_System3.Services
{
    public class BikeService : IBikeService
    {
        private readonly IBikeRepository _bikeRepository;

        public BikeService(IBikeRepository bikeRepository)
        {
            _bikeRepository = bikeRepository;
        }
    }
}
