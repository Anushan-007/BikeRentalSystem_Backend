using BikeRental_System3.IRepository;
using BikeRental_System3.IService;

namespace BikeRental_System3.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;

        public ImageService(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }
    }
}
