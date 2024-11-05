using BikeRental_System3.Models;

namespace BikeRental_System3.IRepository
{
    public interface IBikeRepository
    {
         Task<Bike> AddBike(Bike bike);
    }
}
