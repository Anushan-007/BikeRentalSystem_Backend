﻿using System.ComponentModel.DataAnnotations;

namespace BikeRental_System3.Models
{
    public class Bike
    {
        [Key]
        public Guid Id { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public int RentPerHour { get; set; }

        public List<BikeUnit> BikeUnits { get; set; } = new List<BikeUnit>();
        public ICollection<RentalRequest>? RentalRequests { get; set; }
      
    }
}


//public ICollection<Image>? Images { get; set; }
//public decimal RatePerHour { get; set; }
//public string? Image {  get; set; }