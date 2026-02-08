using BikeRental_System3.Models;
using System.ComponentModel.DataAnnotations;

namespace BikeRental_System3.DTOs.Request
{
    public class ChangeRoleRequest
    {
        [Required(ErrorMessage = "New role is required")]
        [EnumDataType(typeof(Roles), ErrorMessage = "Invalid role specified")]
        public Roles NewRole { get; set; }
    }
}