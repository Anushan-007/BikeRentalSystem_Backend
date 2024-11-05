﻿// <auto-generated />
using System;
using BikeRental_System3.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BikeRental_System3.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20241105101943_EntityAdded1")]
    partial class EntityAdded1
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BikeRental_System3.Models.Bike", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Brand")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("RatePerHour")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Bikes");
                });

            modelBuilder.Entity("BikeRental_System3.Models.Image", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BikeId")
                        .HasColumnType("int");

                    b.Property<string>("ImagePath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BikeId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("BikeRental_System3.Models.Inventory", b =>
                {
                    b.Property<string>("RegistrationNumber")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("Availability")
                        .HasColumnType("bit");

                    b.Property<int>("BikeId")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("YearofManufacture")
                        .HasColumnType("int");

                    b.HasKey("RegistrationNumber");

                    b.HasIndex("BikeId");

                    b.ToTable("Inventories");
                });

            modelBuilder.Entity("BikeRental_System3.Models.RentalRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("Payment")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("RegistrationNumber")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("RentalOut")
                        .HasColumnType("datetime2");

                    b.Property<int>("RentalRequestId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("RentalReturn")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("RegistrationNumber");

                    b.HasIndex("RentalRequestId")
                        .IsUnique();

                    b.ToTable("RentalRecords");
                });

            modelBuilder.Entity("BikeRental_System3.Models.RentalRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BikeId")
                        .HasColumnType("int");

                    b.Property<string>("NicNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("RequestTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<bool?>("UserAlert")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("BikeId");

                    b.HasIndex("NicNumber");

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

                    b.Property<bool>("IsBlocked")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
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

            modelBuilder.Entity("BikeRental_System3.Models.Image", b =>
                {
                    b.HasOne("BikeRental_System3.Models.Bike", "Bike")
                        .WithMany("Images")
                        .HasForeignKey("BikeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Bike");
                });

            modelBuilder.Entity("BikeRental_System3.Models.Inventory", b =>
                {
                    b.HasOne("BikeRental_System3.Models.Bike", "Bike")
                        .WithMany("Inventory")
                        .HasForeignKey("BikeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Bike");
                });

            modelBuilder.Entity("BikeRental_System3.Models.RentalRecord", b =>
                {
                    b.HasOne("BikeRental_System3.Models.Inventory", "inventory")
                        .WithMany("RentalRecords")
                        .HasForeignKey("RegistrationNumber");

                    b.HasOne("BikeRental_System3.Models.RentalRequest", "RentalRequest")
                        .WithOne("RentalRecord")
                        .HasForeignKey("BikeRental_System3.Models.RentalRecord", "RentalRequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RentalRequest");

                    b.Navigation("inventory");
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
                        .HasForeignKey("NicNumber")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Bike");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BikeRental_System3.Models.Bike", b =>
                {
                    b.Navigation("Images");

                    b.Navigation("Inventory");

                    b.Navigation("RentalRequests");
                });

            modelBuilder.Entity("BikeRental_System3.Models.Inventory", b =>
                {
                    b.Navigation("RentalRecords");
                });

            modelBuilder.Entity("BikeRental_System3.Models.RentalRequest", b =>
                {
                    b.Navigation("RentalRecord");
                });

            modelBuilder.Entity("BikeRental_System3.Models.User", b =>
                {
                    b.Navigation("RentalRequest");
                });
#pragma warning restore 612, 618
        }
    }
}
