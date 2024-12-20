﻿using BikeRental_System3.Models;

namespace BikeRental_System3.DTOs.Response
{
    public class BikeUnitResponse
    {
        public Guid UnitId { get; set; }
        public Guid BikeId { get; set; }
        public string RegistrationNumber { get; set; }
        public int Year { get; set; }
        public bool Availability { get; set; } = true;
        public List<ImageResponse> Images { get; set; }
    }
}
