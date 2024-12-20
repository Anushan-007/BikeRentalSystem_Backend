﻿using BikeRental_System3.Models;

namespace BikeRental_System3.DTOs.Response
{
    public class BikeResponse
    {
        public Guid Id { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public int RentPerHour { get; set; }
        public List<BikeUnitResponse> BikeUnits { get; set; }

    }
}


//public decimal RatePerHour { get; set; }
//public string? Image {  get; set; }