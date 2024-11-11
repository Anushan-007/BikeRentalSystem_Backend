using BikeRental_System3.Models;

namespace BikeRental_System3.IRepository
{
    public interface IBikeRepository
    {
         Task<Bike> AddBike(Bike bike);
        Task<List<Bike>> GetAllBikes();
        Task<Bike> GetBikeById(Guid Id);
        Task<Bike> UpdateBike(Bike bike);
        Task<string> DeleteBike(Bike bike);


    }
}
