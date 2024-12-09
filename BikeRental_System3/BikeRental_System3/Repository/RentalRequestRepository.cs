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




        //public async Task<RentalRequest> GetRentalRequest(Guid id)
        //{
        //    var request = await _context.RentalRequests.Include(r => r.Bike).SingleOrDefaultAsync(u => u.Id == id);

        //    if (request == null)
        //    {
        //        throw new Exception();
        //    }

        //    return request;
        //}

        public async Task<List<RentalRequest>> GetRentalRequestbyNic(string nic)
        {
            var data = await _context.RentalRequests.Where(x => x.NicNumber == nic).ToListAsync();
            return data;
        }


        public async Task<RentalRequest> GetRentalRequest(Guid id)
        {
            var request = await _context.RentalRequests
                .Include(r => r.Bike)
                .SingleOrDefaultAsync(u => u.Id == id);

            if (request == null)
            {
                throw new Exception("Rental request not found.");
            }

            return request;
        }


        public async Task<RentalRequest> UpdateRentalRequest(RentalRequest rentalRequest)
        {
            var data = _context.RentalRequests.Update(rentalRequest);
            await _context.SaveChangesAsync();

            return data.Entity;
        }




        public async Task<string> DeleteRentalRequest(Guid id)
        {
            var request = await _context.RentalRequests.FindAsync(id);
            if (request == null)
            {
                throw new ArgumentException();
            }

            _context.RentalRequests.Remove(request);
            await _context.SaveChangesAsync();

            return "Successfully Deleted...";
        }

        public async Task<int> GetPendingRentalRequestsCountAsync()
        {
            return await _context.RentalRequests
                .Where(r => r.Status == Status.Pending)
                .CountAsync();
        }

    }
   
}
