using BikeRental_System3.Data;
using BikeRental_System3.DTOs.Response;
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


        public async Task<List<BikeUnit>> GetAvailablityUnits(bool? availability)
        {
            if (availability == null )
            {
                var data = await _context.BikeUnits.ToListAsync();
                return data;
            }
            else if (availability == true)
            {
                return await _context.BikeUnits.Include(i => i.Bike).Where(u => (u.Availability == true)).ToListAsync();
            }
            else if(availability == false)
            {
                return await _context.BikeUnits.Include(i => i.Bike).Where(u => (u.Availability == false)).ToListAsync();
            }

            return null;

        }

        public async Task<BikeUnit> GetBikeUnitById(string regNo)
        {
            var data = await _context.BikeUnits.FirstOrDefaultAsync(x => x.RegistrationNumber == regNo);
            return data;
        }

        public async Task<Message> DeleteBikeUnit(BikeUnit bikeUnits)
        {
            var data =  _context.BikeUnits.Remove(bikeUnits);
            await _context.SaveChangesAsync();
            if (data == null)
            {
                throw new NotFoundException($"User with NIC Number {bikeUnits} was not found.");

            }
            var message = new Message
            {
                text = "Successfully Deleted"
            };
            return message;
        }



        public async Task<int> TotalBikesCount()
        {
            var data = await _context.BikeUnits.CountAsync();
            return data;
        }

        public async Task<int> GetAvailableBikeUnitsCountAsync()
        {
            return await _context.BikeUnits
                .Where(b => b.Availability)  // Filtering only available and not deleted bike units
                .CountAsync();
        }

        public async Task<int> GetUnavailableBikeUnitsCountAsync()
        {
            return await _context.BikeUnits.CountAsync(bu => bu.Availability == false);
        }

        public class NotFoundException : Exception
        {
            public NotFoundException(string message) : base(message) { }
        }


    }
}
