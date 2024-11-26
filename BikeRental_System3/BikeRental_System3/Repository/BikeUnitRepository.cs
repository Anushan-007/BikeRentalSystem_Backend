using BikeRental_System3.Data;
using BikeRental_System3.IRepository;
using BikeRental_System3.Models;
using Microsoft.EntityFrameworkCore;

namespace BikeRental_System3.Repository
{
    public class BikeUnitRepository : IBikeUnitRepository
    {
        private readonly AppDbContext _context;

        public BikeUnitRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<List<BikeUnit>> GetInventoryUnits(bool? availability, Guid? bikeId)
        {
            if (availability == null || bikeId == null)
            {
                var data = await _context.BikeUnits.ToListAsync();
                return data;
            }
            return await _context.BikeUnits.Include(i => i.Bike).Where(u => (u.Availability == availability) && (u.BikeId == bikeId)).ToListAsync();

        }

        public async Task<BikeUnit> GetInventoryUnit(string RegistrationNumber)
        {
            var data = await _context.BikeUnits.Include(i => i.Bike).SingleOrDefaultAsync(u => u.RegistrationNumber == RegistrationNumber);

            if (data == null)
            {
                throw new Exception();
            }

            return data;
        }

        public async Task<BikeUnit> PutInventoryUnit(BikeUnit inventoryUnit)
        {
            _context.Entry(inventoryUnit).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return inventoryUnit;
        }

    }
}
