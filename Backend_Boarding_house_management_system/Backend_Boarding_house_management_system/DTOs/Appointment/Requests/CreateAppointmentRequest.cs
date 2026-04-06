using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Appointment.Requests
{
    public class CreateAppointmentRequest
    {
        [Required]
        public string PropertyId { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public DateTime AppointmentDateTime { get; set; }

        public string Status { get; set; } = "Pending";

        public string? Note { get; set; }
    }
}
