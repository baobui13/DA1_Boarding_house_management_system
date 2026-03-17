using System.ComponentModel.DataAnnotations;

namespace Backend_Boarding_house_management_system.DTOs.Appointment.Requests
{
    public class DeleteAppointmentRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
