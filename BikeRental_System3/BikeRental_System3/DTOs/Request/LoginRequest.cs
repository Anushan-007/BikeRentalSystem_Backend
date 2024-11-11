﻿using System.ComponentModel.DataAnnotations;

namespace BikeRental_System3.DTOs.Request
{
    public class LoginRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
