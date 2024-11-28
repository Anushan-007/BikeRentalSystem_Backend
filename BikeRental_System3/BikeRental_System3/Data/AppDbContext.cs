using BikeRental_System3.Models;
using Microsoft.EntityFrameworkCore;

namespace BikeRental_System3.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<BikeUnit>()
        //        .HasMany(b => b.Images)
        //        .WithOne(i => i.BikeUnit)
        //        .HasForeignKey(b => b.UnitId);

        //    modelBuilder.Entity<Bike>()
        //        .HasMany(b => b.BikeUnits)
        //        .WithOne(bu => bu.Bike)
        //        .HasForeignKey(bu => bu.BikeId)
        //        .OnDelete(DeleteBehavior.Cascade);

        //                modelBuilder.Entity<Bike>()
        //       .HasMany(b => b.RentalRequests)
        //       .WithOne(r => r.Bike)
        //       .HasForeignKey(r => r.BikeId)
        //       .OnDelete(DeleteBehavior.Cascade);

        //    //    modelBuilder.Entity<BikeUnit>()
        //    //.HasMany(bu => bu.Images)
        //    //.WithOne(bi => bi.BikeUnit)
        //    //.HasForeignKey(bi => bi.UnitId)
        //    //.OnDelete(DeleteBehavior.Cascade);

        //    modelBuilder.Entity<RentalRequest>()
        //        .HasOne(i => i.BikeUnit)
        //        .WithMany(r => r.RentalRequests)
        //        .HasForeignKey(r => r.BikeUnit);



        //    modelBuilder.Entity<BikeUnit>()
        //        .HasOne(i => i.Bike)
        //        .WithMany(b => b.BikeUnits)
        //        .HasForeignKey(b => b.BikeId);

        //    modelBuilder.Entity<BikeUnit>()
        //      .HasMany(i => i.RentalRecords)
        //      .WithOne(r => r.bikeUnits)
        //      .HasForeignKey(r => r.UnitId);


        //            modelBuilder.Entity<RentalRequest>()
        //      .HasOne(r => r.User)
        //      .WithMany(u => u.RentalRequest)
        //      .HasForeignKey(r => r.NicNumber)
        //      .OnDelete(DeleteBehavior.Cascade);

        //    modelBuilder.Entity<RentalRecord>()
        //        .HasOne(r => r.RentalRequest)
        //        .WithOne(r => r.RentalRecord)
        //        .HasForeignKey<RentalRecord>(r => r.RentalRequestId)
        //        .OnDelete(DeleteBehavior.Cascade);

        //    modelBuilder.Entity<User>()
        //      .HasMany(u => u.RentalRequest)
        //      .WithOne(r => r.User)
        //      .HasForeignKey(r => r.NicNumber);

        //    modelBuilder.Entity<RentalRecord>()
        //      .HasOne(r => r.bikeUnits)
        //      .WithMany(u => u.RentalRecords)
        //      .HasForeignKey(r => r.UnitId);

        //    base.OnModelCreating(modelBuilder);
        //}

        public DbSet<Bike> Bikes { get; set; }
        public DbSet<BikeUnit>BikeUnits { get; set; }
        public DbSet<Image> Images { get; set; }
        //public DbSet<Inventory> Inventories { get; set; }
        public DbSet<RentalRecord> RentalRecords { get; set; }
        public DbSet<RentalRequest> RentalRequests { get; set; }
        public DbSet<User> Users { get; set; }

    }
}
