using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Appointment.Requests
{
    public class UpdateAppointmentRequest
    {
        [Required]
        public string Id { get; set; } = null!;

        public DateTime? AppointmentDateTime { get; set; }

        public string? Status { get; set; }

        public string? Note { get; set; }
    }
}
