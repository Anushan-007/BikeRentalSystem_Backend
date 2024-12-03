﻿// <auto-generated />
using System;
using BikeRental_System3.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BikeRental_System3.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BikeRental_System3.Models.Bike", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Brand")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RentPerHour")
                        .HasColumnType("int");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Bikes");
                });

            modelBuilder.Entity("BikeRental_System3.Models.BikeUnit", b =>
                {
                    b.Property<Guid>("UnitId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Availability")
                        .HasColumnType("bit");

                    b.Property<Guid>("BikeId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("RegistrationNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("UnitId");

                    b.HasIndex("BikeId");

                    b.ToTable("BikeUnits");
                });

            modelBuilder.Entity("BikeRental_System3.Models.Image", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("BikeUnitId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ImagePath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BikeUnitId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("BikeRental_System3.Models.RentalRecord", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("BikeUnitId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal?>("Payment")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("RegistrationNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RentalOut")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("RentalRequestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("RentalReturn")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserNicNumber")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("BikeUnitId");

                    b.HasIndex("RentalRequestId")
                        .IsUnique();

                    b.HasIndex("UserNicNumber");

                    b.ToTable("RentalRecords");
                });

            modelBuilder.Entity("BikeRental_System3.Models.RentalRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("BikeId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("NicNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RequestTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<bool?>("UserAlert")
                        .HasColumnType("bit");

                    b.Property<string>("UserNicNumber")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("BikeId");

                    b.HasIndex("UserNicNumber");

                    b.ToTable("RentalRequests");
                });

            modelBuilder.Entity("BikeRental_System3.Models.User", b =>
                {
                    b.Property<string>("NicNumber")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("AccountCreated")
                        .HasColumnType("datetime2");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ContactNo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("IsBlocked")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProfileImage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("roles")
                        .HasColumnType("int");

                    b.HasKey("NicNumber");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("BikeRental_System3.Models.BikeUnit", b =>
                {
                    b.HasOne("BikeRental_System3.Models.Bike", "Bike")
                        .WithMany("BikeUnits")
                        .HasForeignKey("BikeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Bike");
                });

            modelBuilder.Entity("BikeRental_System3.Models.Image", b =>
                {
                    b.HasOne("BikeRental_System3.Models.BikeUnit", "BikeUnit")
                        .WithMany("Images")
                        .HasForeignKey("BikeUnitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BikeUnit");
                });

            modelBuilder.Entity("BikeRental_System3.Models.RentalRecord", b =>
                {
                    b.HasOne("BikeRental_System3.Models.BikeUnit", "BikeUnit")
                        .WithMany("RentalRecords")
                        .HasForeignKey("BikeUnitId");

                    b.HasOne("BikeRental_System3.Models.RentalRequest", "RentalRequest")
                        .WithOne("RentalRecord")
                        .HasForeignKey("BikeRental_System3.Models.RentalRecord", "RentalRequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BikeRental_System3.Models.User", "User")
                        .WithMany("RentalRecords")
                        .HasForeignKey("UserNicNumber");

                    b.Navigation("BikeUnit");

                    b.Navigation("RentalRequest");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BikeRental_System3.Models.RentalRequest", b =>
                {
                    b.HasOne("BikeRental_System3.Models.Bike", "Bike")
                        .WithMany("RentalRequests")
                        .HasForeignKey("BikeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BikeRental_System3.Models.User", "User")
                        .WithMany("RentalRequest")
                        .HasForeignKey("UserNicNumber");

                    b.Navigation("Bike");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BikeRental_System3.Models.Bike", b =>
                {
                    b.Navigation("BikeUnits");

                    b.Navigation("RentalRequests");
                });

            modelBuilder.Entity("BikeRental_System3.Models.BikeUnit", b =>
                {
                    b.Navigation("Images");

                    b.Navigation("RentalRecords");
                });

            modelBuilder.Entity("BikeRental_System3.Models.RentalRequest", b =>
                {
                    b.Navigation("RentalRecord");
                });

            modelBuilder.Entity("BikeRental_System3.Models.User", b =>
                {
                    b.Navigation("RentalRecords");

                    b.Navigation("RentalRequest");
                });
#pragma warning restore 612, 618
        }
    }
}
