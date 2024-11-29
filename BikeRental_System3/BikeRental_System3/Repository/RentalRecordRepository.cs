using BikeRental_System3.Data;
using BikeRental_System3.IRepository;
using BikeRental_System3.Models;
using Microsoft.EntityFrameworkCore;

namespace BikeRental_System3.Repository
{
    public class RentalRecordRepository : IRentalRecordRepository
    {
        private readonly AppDbContext _context;

        public RentalRecordRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RentalRecord> PostRentalRecord(RentalRecord RentalRecord)
        {
            var data = await _context.RentalRecords.AddAsync(RentalRecord);
            await _context.SaveChangesAsync();

            return data.Entity;
        }

        public async Task<List<RentalRecord>> GetRentalRecords()
        {
            return await _context.RentalRecords.Where(r => r.RentalReturn != null).Include(r => r.RentalRequest).ToListAsync();
        }

        public async Task<RentalRecord> GetRentalRecord(Guid id)
        {
            var request = await _context.RentalRecords.FirstOrDefaultAsync(x => x.Id == id);
            if (request == null)
            {
                throw new Exception();
            }

            return request;
        }

        public async Task<List<RentalRecord>> GetIncompleteRentalRecords()
        {
            var data = await _context.RentalRecords.Where(r => r.RentalReturn == null).Include(r => r.RentalRequest).ToListAsync();
            return data;
        }



    

        public async Task<RentalRecord> UpdateRentalRecord(RentalRecord RentalRecord)
        {
            var data = _context.RentalRecords.Update(RentalRecord);
            await _context.SaveChangesAsync();

            return data.Entity;
        }


    }
}
