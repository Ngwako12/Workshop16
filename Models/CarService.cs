using System;
using System.ComponentModel.DataAnnotations;
//Service booking model
namespace Workshop16.Models
{
    public class CarService
    {
        [Key]
        public int Id { get; set; }

        
        public string CustomerId { get; set; }  // Link to ApplicationUser

        [Required]
        public string VehicleMake { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public int Mileage { get; set; }

        [Required]
        public string VIN { get; set; }

        [Required]
        public string ServiceType { get; set; }  // Type of car service .

        [Required]
        public DateTime AppointmentDate { get; set; }

        public string Status { get; set; } = "Booked"; // Default status

    }
}
