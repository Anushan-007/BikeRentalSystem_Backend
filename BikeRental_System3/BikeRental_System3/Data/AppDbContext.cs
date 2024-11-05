using BikeRental_System3.Models;
using Microsoft.EntityFrameworkCore;

namespace BikeRental_System3.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bike>()
                .HasMany(b => b.Images)
                .WithOne(i => i.Bike)
                .HasForeignKey(b => b.BikeId);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Bike)
                .WithMany(b => b.Inventory)
                .HasForeignKey(b => b.BikeId);

            modelBuilder.Entity<Inventory>()
              .HasMany(i => i.RentalRecords)
              .WithOne(r => r.inventory)
              .HasForeignKey(r => r.RegistrationNumber);

            modelBuilder.Entity<RentalRecord>()
                .HasOne(r => r.RentalRequest)
                .WithOne(r => r.RentalRecord)
                .HasForeignKey<RentalRecord>(r => r.RentalRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
              .HasMany(u => u.RentalRequest)
              .WithOne(r => r.User)
              .HasForeignKey(r => r.NicNumber);

            modelBuilder.Entity<RentalRecord>()
              .HasOne(r => r.inventory)
              .WithMany(u => u.RentalRecords)
              .HasForeignKey(r => r.RegistrationNumber);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Bike> Bikes { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<RentalRecord> RentalRecords { get; set; }
        public DbSet<RentalRequest> RentalRequests { get; set; }
        public DbSet<User> Users { get; set; }

    }
}
