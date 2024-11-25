using BikeRental_System3.Data;
using BikeRental_System3.IRepository;
using BikeRental_System3.Models;
using Microsoft.EntityFrameworkCore;

namespace BikeRental_System3.Repository
{
    public class RentalRequestRepository : IRentalRequestRepository
    {
        private readonly AppDbContext _context;

        public RentalRequestRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<RentalRequest> PostRentalRequest(RentalRequest rentalRequest)
        {
            var data = await _context.RentalRequests.AddAsync(rentalRequest);
            await _context.SaveChangesAsync();

            return data.Entity;
        
        }

        public async Task<List<RentalRequest>> GetRentalRequests()
        {
            return await _context.RentalRequests.Where(r => r.Status == Status.Pending).Include(r => r.Bike).ToListAsync();
        }

        public async Task<List<RentalRequest>> GetRentalRequestsByStatus(Status? status)
        {
            return await _context.RentalRequests.Where(r => r.Status == status).Include(r => r.Bike).ToListAsync();
        }


    }
   
}
