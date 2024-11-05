using BikeRental_System3.Data;
using BikeRental_System3.IRepository;

namespace BikeRental_System3.Repository
{
    public class ImageRepository : IImageRepository
    {
        private readonly AppDbContext _context;

        public ImageRepository(AppDbContext context)
        {
            _context = context;
        }

    }
}
